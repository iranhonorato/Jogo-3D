using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Guaxinim.EditorTools
{
    /// <summary>
    /// Controles do jogo no teclado e no controle (GDD §3.2), num lugar só: o "Como jogar" do menu e o quadro da pausa
    /// montam a mesma tabela (ação, teclado e controle). Os botões do controle seguem o padrão Xbox, como no Input
    /// System (<c>Settings/Input/InputSystem_Actions</c>); a nota dá os equivalentes do PlayStation.
    /// </summary>
    internal static class ControlsTable
    {
        public const string GamepadNote =
            "Controle no padrão Xbox. No PlayStation: A = X, B = Bola, Y = Triângulo e RT = R2.";
        public const string MenusNote = "Nos menus: setas e Enter, ou direcional e A.";

        private const string ActionHeader = "Ação";
        private const string KeyboardHeader = "Teclado";
        private const string GamepadHeader = "Controle";
        private const float HeaderHeightPerFontSize = 1.5f;

        private static readonly Color HeaderColor = new Color32(0xFF, 0xC8, 0x4D, 0xFF); // âmbar dos destaques

        private static readonly Row[] Rows =
        {
            new Row("Mover (silencioso)", "Mover", "WASD", "Analógico"),
            new Row("Correr (faz barulho)", "Correr", "Shift", "RT"),
            new Row("Pegar item e usar a lixeira", "Pegar e esconder", "E", "A"),
            new Row("Largar o último item", "Largar o item", "Q", "B"),
            new Row("Assobiar (distrai os seguranças)", "Assobiar", "F", "Y"),
            new Row("Ver a lista inteira (segurar)", "Lista inteira", "Tab", "Select"),
            new Row("Pausar", "Pausar", "Esc", "Start"),
        };

        /// <summary>Tabela com o nome completo de cada ação (tela "Como jogar").</summary>
        public static void BuildFull(RectTransform area, Columns columns, int fontSize) =>
            Build(area, columns, fontSize, row => row.Action);

        /// <summary>Tabela com nomes curtos, para espaços estreitos (quadro da pausa).</summary>
        public static void BuildCompact(RectTransform area, Columns columns, int fontSize) =>
            Build(area, columns, fontSize, row => row.ShortAction);

        /// <summary>
        /// Cabeçalho em âmbar e as linhas de comandos dentro da área. Cada coluna é um texto próprio, sem quebra de
        /// linha e com o mesmo tamanho de letra, para as três colunas ficarem alinhadas linha a linha.
        /// </summary>
        private static void Build(RectTransform area, Columns columns, int fontSize, Func<Row, string> actionLabel)
        {
            float headerHeight = fontSize * HeaderHeightPerFontSize;
            AddColumn(area, "Action", ActionHeader, JoinRows(actionLabel), columns.Action, fontSize, headerHeight);
            AddColumn(area, "Keyboard", KeyboardHeader, JoinRows(row => row.Keyboard), columns.Keyboard, fontSize,
                headerHeight);
            AddColumn(area, "Gamepad", GamepadHeader, JoinRows(row => row.Gamepad), columns.Gamepad, fontSize,
                headerHeight);
        }

        private static string JoinRows(Func<Row, string> cell)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < Rows.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append('\n');
                }

                builder.Append(cell(Rows[i]));
            }

            return builder.ToString();
        }

        private static void AddColumn(RectTransform area, string name, string header, string body, Vector2 span,
            int fontSize, float headerHeight)
        {
            Text headerText = EditorUI.CreateTitle(name + "Header", area, header, fontSize, HeaderColor);
            headerText.alignment = TextAnchor.LowerLeft;
            var headerRect = (RectTransform)headerText.transform;
            headerRect.anchorMin = new Vector2(span.x, 1f);
            headerRect.anchorMax = new Vector2(span.y, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.sizeDelta = new Vector2(0f, headerHeight);
            headerRect.anchoredPosition = Vector2.zero;

            Text bodyText = EditorUI.CreateText(name, area, body, fontSize, Color.white);
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.horizontalOverflow = HorizontalWrapMode.Overflow; // sem quebra: as linhas das colunas batem
            bodyText.verticalOverflow = VerticalWrapMode.Overflow;
            var bodyRect = (RectTransform)bodyText.transform;
            bodyRect.anchorMin = new Vector2(span.x, 0f);
            bodyRect.anchorMax = new Vector2(span.y, 1f);
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = new Vector2(0f, -headerHeight);
        }

        /// <summary>Início e fim de cada coluna, em fração da largura da área (0 a 1).</summary>
        internal readonly struct Columns
        {
            public Columns(Vector2 action, Vector2 keyboard, Vector2 gamepad)
            {
                Action = action;
                Keyboard = keyboard;
                Gamepad = gamepad;
            }

            public Vector2 Action { get; }
            public Vector2 Keyboard { get; }
            public Vector2 Gamepad { get; }
        }

        private readonly struct Row
        {
            public Row(string action, string shortAction, string keyboard, string gamepad)
            {
                Action = action;
                ShortAction = shortAction;
                Keyboard = keyboard;
                Gamepad = gamepad;
            }

            public string Action { get; }
            public string ShortAction { get; }
            public string Keyboard { get; }
            public string Gamepad { get; }
        }
    }
}
