using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace Tartaria.Input
{
    /// <summary>
    /// Registers Logitech gamepad HID layouts so they are recognized as standard
    /// Gamepads even in DirectInput mode. XInput mode already maps to &lt;Gamepad&gt;
    /// automatically — this covers the DirectInput (D) switch position on:
    ///   • Logitech F310 (wired)
    ///   • Logitech F510 (wired, rumble)
    ///   • Logitech F710 (wireless)
    ///
    /// Also detects Logitech Dual Action and older models via product name heuristics.
    /// Runs once in [RuntimeInitializeOnLoadMethod] before any input is read.
    /// </summary>
    public static class LogitechControllerSupport
    {
        // Logitech vendor ID (all consumer gamepads)
        const int LogitechVendorId = 0x046D;

        // Product IDs — DirectInput mode
        const int F310_DInput  = 0xC216;
        const int F510_DInput  = 0xC218;
        const int F710_DInput  = 0xC219;
        const int DualAction   = 0xC216; // Same PID as F310 DInput on some revisions

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            // F310 DirectInput
            InputSystem.RegisterLayoutMatcher<Gamepad>(
                new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithCapability("vendorId", LogitechVendorId)
                    .WithCapability("productId", F310_DInput));

            // F510 DirectInput
            InputSystem.RegisterLayoutMatcher<Gamepad>(
                new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithCapability("vendorId", LogitechVendorId)
                    .WithCapability("productId", F510_DInput));

            // F710 DirectInput
            InputSystem.RegisterLayoutMatcher<Gamepad>(
                new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithCapability("vendorId", LogitechVendorId)
                    .WithCapability("productId", F710_DInput));

            // Catch-all: any Logitech HID gamepad by product name substring
            InputSystem.onDeviceChange += OnDeviceChange;

            Debug.Log("[Logitech] Controller layout matchers registered (F310/F510/F710 DirectInput).");
        }

        static void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change != InputDeviceChange.Added) return;
            if (device is Gamepad) return; // Already recognized — nothing to do

            var desc = device.description;
            if (string.IsNullOrEmpty(desc.product)) return;

            string product = desc.product.ToUpperInvariant();

            bool isLogitech = product.Contains("LOGITECH") ||
                              product.Contains("F310") ||
                              product.Contains("F510") ||
                              product.Contains("F710") ||
                              product.Contains("DUAL ACTION") ||
                              product.Contains("RUMBLEPAD");

            if (!isLogitech) return;

            // Force re-creation as Gamepad layout
            InputSystem.RemoveDevice(device);
            InputSystem.AddDevice(new InputDeviceDescription
            {
                interfaceName = desc.interfaceName,
                deviceClass = "Gamepad",
                manufacturer = desc.manufacturer,
                product = desc.product,
                serial = desc.serial,
                version = desc.version,
                capabilities = desc.capabilities
            });

            Debug.Log($"[Logitech] Re-registered '{desc.product}' as Gamepad.");
        }
    }
}
