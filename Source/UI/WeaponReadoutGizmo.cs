using System;
using System.Collections.Generic;
using SOS2WeaponReadouts.Domain;
using UnityEngine;
using Verse;

namespace SOS2WeaponReadouts.UI
{
    internal sealed class WeaponReadoutGizmo : Gizmo
    {
        private const float Width = 210f;
        private const float GizmoHeight = 75f;
        private const float Padding = 4f;
        private const float TitleRowHeight = 19f;
        private const float DataRowHeight = 24f;

        private readonly WeaponReadout readout;
        private readonly ReadoutPresentation presentation;
        private readonly ReadoutLabels labels;
        private readonly int tooltipId;

        internal WeaponReadoutGizmo(
            WeaponReadout readout,
            ReadoutPresentation presentation,
            ReadoutLabels labels,
            int tooltipId)
        {
            this.readout = readout ??
                throw new ArgumentNullException(nameof(readout));
            this.presentation = presentation ??
                throw new ArgumentNullException(nameof(presentation));
            this.labels = labels ??
                throw new ArgumentNullException(nameof(labels));
            this.tooltipId = tooltipId;
            Order = -120f;
        }

        public override float GetWidth(float maxWidth)
        {
            return Width;
        }

        public override GizmoResult GizmoOnGUI(
            Vector2 topLeft,
            float maxWidth,
            GizmoRenderParms parms)
        {
            float width = GetWidth(maxWidth);
            var outer = new Rect(
                topLeft.x,
                topLeft.y,
                width,
                GizmoHeight);
            var content = outer.ContractedBy(Padding);
            Widgets.DrawWindowBackground(outer);

            GameFont previousFont = Text.Font;
            TextAnchor previousAnchor = Text.Anchor;
            Color previousColor = GUI.color;
            try
            {
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperLeft;
                float y = content.y;
                DrawRow(
                    content,
                    ref y,
                    "SOS2WR.Gizmo.Title".Translate(),
                    Color.white,
                    TitleRowHeight);

                DrawRow(
                    content,
                    ref y,
                    CurrentHeatText(),
                    HasThermalWarning()
                        ? ColorLibrary.RedReadable
                        : Color.white,
                    DataRowHeight);
                DrawRow(
                    content,
                    ref y,
                    labels.HeatPerShot + ": " +
                        ReadoutFormatter.FormatNumber(
                            readout.HeatPerShot) +
                        " " + labels.HeatUnits,
                    Color.white,
                    DataRowHeight);

                TooltipHandler.TipRegion(
                    outer,
                    () => string.Join(
                        Environment.NewLine,
                        BuildTooltipLines()),
                    tooltipId);
            }
            finally
            {
                GUI.color = previousColor;
                Text.Anchor = previousAnchor;
                Text.Font = previousFont;
            }

            return new GizmoResult(GizmoState.Clear);
        }

        private string CurrentHeatText()
        {
            NetworkReadout network = readout.Network;
            if (!presentation.ShowNetworkComparison ||
                network == null)
            {
                return string.Empty;
            }

            if (!network.ThermalNetworkConnected)
            {
                return labels.ThermalDisconnected;
            }

            return labels.CurrentHeat + ": " +
                ReadoutFormatter.FormatNumber(network.Used) +
                " / " +
                ReadoutFormatter.FormatNumber(network.Capacity) +
                " " + labels.HeatUnits;
        }

        private bool HasThermalWarning()
        {
            NetworkReadout network = readout.Network;
            return network != null &&
                (!network.ThermalNetworkConnected ||
                 !network.BridgeConnected ||
                 network.Used + readout.HeatPerShot >
                    network.Capacity + 0.001f);
        }

        private IEnumerable<string> BuildTooltipLines()
        {
            return ReadoutFormatter.BuildMissingLines(
                string.Empty,
                readout,
                new ReadoutPresentation
                {
                    ShowElectricalDraw = false,
                    ShowNetworkComparison =
                        presentation.ShowNetworkComparison
                },
                labels);
        }

        private static void DrawRow(
            Rect content,
            ref float y,
            string text,
            Color color,
            float height)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            GUI.color = color;
            Widgets.Label(
                new Rect(
                    content.x,
                    y,
                    content.width,
                    height),
                text);
            y += height;
        }
    }
}
