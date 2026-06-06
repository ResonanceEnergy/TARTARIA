# TARTARIA — CI/CD Integration Guide

## Overview

This document provides integration templates for TARTARIA's automated test suite with popular CI/CD platforms.

---

## GitHub Actions

### Workflow File: `.github/workflows/test.yml`

```yaml
name: TARTARIA - Automated Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]
  workflow_dispatch:

env:
  UNITY_VERSION: 6000.0.36f1
  UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}

jobs:
  test:
    runs-on: windows-latest
    timeout-minutes: 30
    
    steps:
    - name: Checkout Repository
      uses: actions/checkout@v3
      with:
        lfs: true
    
    - name: Cache Unity Library
      uses: actions/cache@v3
      with:
        path: Library
        key: Library-${{ hashFiles('Assets/**', 'Packages/**', 'ProjectSettings/**') }}
        restore-keys: |
          Library-
    
    - name: Setup Unity
      uses: game-ci/unity-builder@v4
      with:
        unityVersion: ${{ env.UNITY_VERSION }}
    
    - name: Run Automated Tests
      run: |
        pwsh -File run-automated-tests.ps1
      continue-on-error: true
    
    - name: Parse Test Results
      if: always()
      run: |
        if (Test-Path "Logs/test-metrics-latest.json") {
          $metrics = Get-Content "Logs/test-metrics-latest.json" | ConvertFrom-Json
          Write-Host "::notice title=Test Results::Passed: $($metrics.TotalPass), Failed: $($metrics.TotalFail), Warnings: $($metrics.TotalWarn)"
          
          if ($metrics.TotalFail -gt 0) {
            Write-Host "::error title=Test Failures::$($metrics.TotalFail) tests failed"
            exit 1
          }
        }
      shell: pwsh
    
    - name: Upload Test Reports
      if: always()
      uses: actions/upload-artifact@v3
      with:
        name: test-reports-${{ github.sha }}
        path: |
          Logs/Reports/
          Logs/test-run.log
          Logs/test-metrics-latest.json
        retention-days: 30
    
    - name: Upload Test Logs on Failure
      if: failure()
      uses: actions/upload-artifact@v3
      with:
        name: test-logs-${{ github.sha }}
        path: Logs/*.log
        retention-days: 7
    
    - name: Comment PR with Test Results
      if: github.event_name == 'pull_request' && always()
      uses: actions/github-script@v6
      with:
        script: |
          const fs = require('fs');
          const metricsPath = 'Logs/test-metrics-latest.json';
          
          if (fs.existsSync(metricsPath)) {
            const metrics = JSON.parse(fs.readFileSync(metricsPath, 'utf8'));
            
            const status = metrics.TotalFail === 0 ? '✅ PASSED' : '❌ FAILED';
            const body = `## TARTARIA Test Results ${status}
            
            | Metric | Count |
            |--------|-------|
            | ✅ Passed | ${metrics.TotalPass} |
            | ❌ Failed | ${metrics.TotalFail} |
            | ⚠️ Warnings | ${metrics.TotalWarn} |
            | 📊 Duration | ${metrics.Duration.toFixed(2)}s |
            
            [View Full Report](https://github.com/${{ github.repository }}/actions/runs/${{ github.run_id }})`;
            
            github.rest.issues.createComment({
              issue_number: context.issue.number,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: body
            });
          }
```

---

## Jenkins

### Jenkinsfile

```groovy
pipeline {
    agent {
        label 'windows-unity'
    }
    
    environment {
        UNITY_PATH = 'C:\\Program Files\\Unity\\Hub\\Editor\\6000.0.36f1\\Editor\\Unity.exe'
        PROJECT_PATH = "${WORKSPACE}"
    }
    
    options {
        timeout(time: 30, unit: 'MINUTES')
        buildDiscarder(logRotator(numToKeepStr: '10'))
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
        
        stage('Cache Setup') {
            steps {
                // Cache Unity Library folder
                cache(maxCacheSize: 5000, caches: [
                    arbitraryFileCache(
                        path: 'Library',
                        cacheValidityDecidingFile: 'Assets/**/*.cs'
                    )
                ]) {
                    echo 'Unity Library cached'
                }
            }
        }
        
        stage('Run Tests') {
            steps {
                powershell '''
                    $ErrorActionPreference = "Continue"
                    .\\run-automated-tests.ps1
                    
                    if (Test-Path "Logs\\test-metrics-latest.json") {
                        $metrics = Get-Content "Logs\\test-metrics-latest.json" | ConvertFrom-Json
                        Write-Host "Test Results: Passed=$($metrics.TotalPass), Failed=$($metrics.TotalFail), Warnings=$($metrics.TotalWarn)"
                    }
                '''
            }
        }
        
        stage('Parse Results') {
            steps {
                script {
                    def metricsFile = 'Logs/test-metrics-latest.json'
                    if (fileExists(metricsFile)) {
                        def metrics = readJSON file: metricsFile
                        
                        currentBuild.description = """
                            Tests: ${metrics.TotalPass} passed, ${metrics.TotalFail} failed
                            Duration: ${metrics.Duration.round(2)}s
                        """
                        
                        if (metrics.TotalFail > 0) {
                            currentBuild.result = 'UNSTABLE'
                        }
                    }
                }
            }
        }
    }
    
    post {
        always {
            archiveArtifacts artifacts: 'Logs/Reports/**', allowEmptyArchive: true
            archiveArtifacts artifacts: 'Logs/*.log', allowEmptyArchive: true
            
            publishHTML(target: [
                reportDir: 'Logs/Reports',
                reportFiles: 'TestReport-Latest.html',
                reportName: 'TARTARIA Test Report',
                keepAll: true,
                alwaysLinkToLastBuild: true,
                allowMissing: false
            ])
        }
        
        failure {
            emailext(
                subject: "TARTARIA Tests Failed: ${env.JOB_NAME} #${env.BUILD_NUMBER}",
                body: """
                    Test execution failed.
                    
                    Build: ${env.BUILD_URL}
                    Console: ${env.BUILD_URL}console
                    
                    Check the attached test report for details.
                """,
                to: 'tartaria-dev@example.com',
                attachLog: true
            )
        }
    }
}
```

---

## Azure DevOps

### azure-pipelines.yml

```yaml
trigger:
  branches:
    include:
    - main
    - develop

pool:
  vmImage: 'windows-latest'

variables:
  unityVersion: '6000.0.36f1'
  unityPath: 'C:\Program Files\Unity\Hub\Editor\$(unityVersion)\Editor\Unity.exe'

stages:
- stage: Test
  displayName: 'Run Automated Tests'
  jobs:
  - job: TestJob
    displayName: 'Execute TARTARIA Tests'
    timeoutInMinutes: 30
    
    steps:
    - checkout: self
      lfs: true
      clean: true
    
    - task: Cache@2
      displayName: 'Cache Unity Library'
      inputs:
        key: 'unity-library | "$(Agent.OS)" | Assets/**/*.cs'
        path: 'Library'
        restoreKeys: |
          unity-library | "$(Agent.OS)"
    
    - task: PowerShell@2
      displayName: 'Run TARTARIA Tests'
      inputs:
        filePath: 'run-automated-tests.ps1'
        errorActionPreference: 'continue'
        pwsh: true
      continueOnError: true
    
    - task: PowerShell@2
      displayName: 'Parse Test Results'
      condition: always()
      inputs:
        targetType: 'inline'
        script: |
          if (Test-Path "Logs/test-metrics-latest.json") {
            $metrics = Get-Content "Logs/test-metrics-latest.json" | ConvertFrom-Json
            
            Write-Host "##vso[task.logissue type=warning]Test Results: Passed=$($metrics.TotalPass), Failed=$($metrics.TotalFail), Warnings=$($metrics.TotalWarn)"
            
            if ($metrics.TotalFail -gt 0) {
              Write-Host "##vso[task.logissue type=error]$($metrics.TotalFail) tests failed"
              Write-Host "##vso[task.complete result=Failed;]Tests failed"
            } else {
              Write-Host "##vso[task.complete result=Succeeded;]All tests passed"
            }
          }
        pwsh: true
    
    - task: PublishBuildArtifacts@1
      displayName: 'Publish Test Reports'
      condition: always()
      inputs:
        pathToPublish: 'Logs/Reports'
        artifactName: 'test-reports-$(Build.BuildNumber)'
    
    - task: PublishBuildArtifacts@1
      displayName: 'Publish Test Logs'
      condition: failed()
      inputs:
        pathToPublish: 'Logs'
        artifactName: 'test-logs-$(Build.BuildNumber)'
    
    - task: PublishTestResults@2
      displayName: 'Publish Test Results'
      condition: always()
      inputs:
        testResultsFormat: 'JUnit'
        testResultsFiles: 'Logs/Reports/TestReport-*.json'
        mergeTestResults: true
        failTaskOnFailedTests: true
        testRunTitle: 'TARTARIA Automated Tests'
```

---

## GitLab CI/CD

### .gitlab-ci.yml

```yaml
stages:
  - test
  - report

variables:
  UNITY_VERSION: "6000.0.36f1"
  GIT_LFS_SKIP_SMUDGE: "1"

cache:
  key: unity-library-$CI_COMMIT_REF_SLUG
  paths:
    - Library/

test:
  stage: test
  tags:
    - windows
    - unity
  timeout: 30m
  
  before_script:
    - git lfs pull
  
  script:
    - pwsh -File run-automated-tests.ps1
  
  after_script:
    - |
      if (Test-Path "Logs/test-metrics-latest.json") {
        $metrics = Get-Content "Logs/test-metrics-latest.json" | ConvertFrom-Json
        Write-Host "Test Results: Passed=$($metrics.TotalPass), Failed=$($metrics.TotalFail)"
        
        if ($metrics.TotalFail -gt 0) {
          exit 1
        }
      }
  
  artifacts:
    when: always
    expire_in: 30 days
    paths:
      - Logs/Reports/
      - Logs/*.log
    reports:
      junit: Logs/Reports/TestReport-*.json

publish_report:
  stage: report
  tags:
    - windows
  dependencies:
    - test
  
  script:
    - |
      if (Test-Path "Logs/Reports/TestReport-Latest.html") {
        Copy-Item "Logs/Reports/TestReport-Latest.html" public/index.html
      }
  
  artifacts:
    paths:
      - public
  
  only:
    - main
    - develop
```

---

## CircleCI

### .circleci/config.yml

```yaml
version: 2.1

executors:
  windows-unity:
    machine:
      image: windows-server-2022-gui:current
    resource_class: windows.medium

jobs:
  test:
    executor: windows-unity
    
    steps:
      - checkout
      
      - restore_cache:
          keys:
            - unity-library-v1-{{ checksum "Assets/**/*.cs" }}
            - unity-library-v1-
      
      - run:
          name: Run TARTARIA Tests
          command: |
            pwsh -File run-automated-tests.ps1
          no_output_timeout: 30m
      
      - run:
          name: Parse Results
          command: |
            if (Test-Path "Logs/test-metrics-latest.json") {
              $metrics = Get-Content "Logs/test-metrics-latest.json" | ConvertFrom-Json
              
              if ($metrics.TotalFail -gt 0) {
                Write-Error "$($metrics.TotalFail) tests failed"
                exit 1
              }
            }
          shell: pwsh
          when: always
      
      - save_cache:
          key: unity-library-v1-{{ checksum "Assets/**/*.cs" }}
          paths:
            - Library
      
      - store_artifacts:
          path: Logs/Reports
          destination: test-reports
      
      - store_artifacts:
          path: Logs/*.log
          destination: test-logs

workflows:
  test:
    jobs:
      - test:
          filters:
            branches:
              only:
                - main
                - develop
```

---

## Docker Integration

### Dockerfile (for containerized testing)

```dockerfile
FROM unityci/editor:windows-2022-6000.0.36f1

WORKDIR /project

COPY . .

RUN pwsh -Command "Set-ExecutionPolicy Bypass -Scope Process; .\run-automated-tests.ps1"

CMD ["pwsh", "-File", "run-automated-tests.ps1"]
```

---

## Performance Thresholds

Configure performance gates in CI/CD:

```yaml
# Example: GitHub Actions with performance checks
- name: Check Performance Gates
  run: |
    $metrics = Get-Content "Logs/test-metrics-latest.json" | ConvertFrom-Json
    $report = Get-Content "Logs/Reports/TestReport-Latest.json" | ConvertFrom-Json
    
    $avgFPS = $report.performance.AvgFPS
    $heapMB = $report.performance.HeapMemoryMB
    
    if ($avgFPS -lt 60) {
      Write-Host "::error title=Performance::FPS below threshold: $avgFPS (expected ≥60)"
      exit 1
    }
    
    if ($heapMB -gt 512) {
      Write-Host "::error title=Performance::Heap memory exceeded: $($heapMB)MB (expected ≤512MB)"
      exit 1
    }
    
    Write-Host "::notice title=Performance::All gates passed (FPS=$avgFPS, Heap=$($heapMB)MB)"
  shell: pwsh
```

---

## Slack/Discord Notifications

### Slack Webhook

```powershell
# Add to post-test script
$webhookUrl = $env:SLACK_WEBHOOK_URL
$metrics = Get-Content "Logs/test-metrics-latest.json" | ConvertFrom-Json

$status = if ($metrics.TotalFail -eq 0) { ":white_check_mark: PASSED" } else { ":x: FAILED" }
$color = if ($metrics.TotalFail -eq 0) { "good" } else { "danger" }

$payload = @{
    attachments = @(
        @{
            color = $color
            title = "TARTARIA Test Results $status"
            fields = @(
                @{ title = "Passed"; value = $metrics.TotalPass; short = $true }
                @{ title = "Failed"; value = $metrics.TotalFail; short = $true }
                @{ title = "Warnings"; value = $metrics.TotalWarn; short = $true }
                @{ title = "Duration"; value = "$($metrics.Duration.ToString('F2'))s"; short = $true }
            )
        }
    )
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Uri $webhookUrl -Method Post -Body $payload -ContentType "application/json"
```

---

## Contact

For CI/CD integration support, contact: **tartaria-dev@example.com**
