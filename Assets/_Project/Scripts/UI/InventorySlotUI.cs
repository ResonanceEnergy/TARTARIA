using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Tartaria.UI
{
    /// <summary>
    /// Single inventory slot UI element — handles click, hover, icon display.
    /// Used by InventoryUIPanel grid. Wire to EventSystem for pointer events.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Elements")]
        [SerializeField] Image iconImage;
        [SerializeField] Text countText;
        [SerializeField] Image highlightBorder;

        [Header("State")]
        int _slotIndex = -1;
        string _itemId;
        int _itemCount;
        bool _isEmpty = true;

        public event Action<int> OnSlotClicked;
        public event Action<int, bool> OnSlotHovered;  // slotIndex, entered

        public void SetSlotIndex(int index)
        {
            _slotIndex = index;
        }

        public void SetItem(string itemId, int count, Sprite icon)
        {
            _itemId = itemId;
            _itemCount = count;
            _isEmpty = false;

            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
            }

            if (countText != null)
            {
                countText.text = count > 1 ? count.ToString() : "";
                countText.enabled = count > 1;
            }
        }

        public void ClearSlot()
        {
            _itemId = null;
            _itemCount = 0;
            _isEmpty = true;

            if (iconImage != null) iconImage.enabled = false;
            if (countText != null) countText.enabled = false;
        }

        public void SetHighlight(bool highlighted)
        {
            if (highlightBorder != null)
                highlightBorder.enabled = highlighted;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_slotIndex >= 0)
                OnSlotClicked?.Invoke(_slotIndex);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_slotIndex >= 0)
            {
                SetHighlight(true);
                OnSlotHovered?.Invoke(_slotIndex, true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_slotIndex >= 0)
            {
                SetHighlight(false);
                OnSlotHovered?.Invoke(_slotIndex, false);
            }
        }

        public string GetItemId() => _itemId;
        public int GetItemCount() => _itemCount;
        public bool IsEmpty() => _isEmpty;
    }
}
