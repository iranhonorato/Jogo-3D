using System.Text;
using Guaxinim.Core;
using Guaxinim.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.UI
{
    /// <summary>
    /// Lista de compras na HUD (GDD §7.1 e §13), no canto superior esquerdo. Normalmente mostra só o que
    /// falta; segurando Tab, mostra a lista inteira com as marcas e os bônus. Marca só na entrega na toca.
    /// </summary>
    public class ShoppingListUI : MonoBehaviour
    {
        private const string Title = "Lista de compras";
        private const string TabHint = "(Tab: lista inteira)";

        [SerializeField] private ShoppingList _shoppingList;

        [Tooltip("Input do guaxinim (ligado na montagem da fase)")]
        [SerializeField] private InputReader _input;

        [SerializeField] private Text _text;

        private readonly StringBuilder _builder = new StringBuilder();
        private bool _isExpanded;

        private void Awake()
        {
            if (_shoppingList == null || _input == null || _text == null)
            {
                Debug.LogError("ShoppingListUI is missing references.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_shoppingList != null && _input != null)
            {
                _shoppingList.OnItemDelivered += HandleItemDelivered;
                _input.OnShowListHeldChanged += HandleShowListHeldChanged;
            }
        }

        private void Start() => Refresh();

        private void OnDisable()
        {
            if (_shoppingList != null && _input != null)
            {
                _shoppingList.OnItemDelivered -= HandleItemDelivered;
                _input.OnShowListHeldChanged -= HandleShowListHeldChanged;
            }
        }

        private void HandleItemDelivered(ItemData item, bool wasOnList) => Refresh();

        private void HandleShowListHeldChanged(bool isHeld)
        {
            _isExpanded = isHeld;
            Refresh();
        }

        private void Refresh()
        {
            ShoppingListProgress progress = _shoppingList.Progress;
            _builder.Clear();
            _builder.Append(Title).Append("  ").Append(progress.DeliveredCount).Append('/').Append(progress.RequiredCount);
            _builder.AppendLine();
            for (int i = 0; i < progress.RequiredCount; i++)
            {
                bool isDelivered = progress.IsDelivered(i);
                if (isDelivered && !_isExpanded)
                {
                    continue;
                }

                _builder.Append(isDelivered ? "[x] " : "[ ] ").AppendLine(progress.GetRequired(i).DisplayName);
            }

            _builder.Append(_isExpanded ? $"Bônus entregues: {progress.BonusCount}" : TabHint);
            _text.text = _builder.ToString();
        }
    }
}
