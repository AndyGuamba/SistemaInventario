using Inventario.Modelos.Entidades;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;

namespace Inventario.API.Reportes
{
    public static class GeneradorPdfInventario
    {
        public static byte[] Generar(IEnumerable<Parabrisa> listaParabrisas)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Orientación horizontal (Landscape) para que entren bien las 7 columnas
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10)); // Letra un pelín más pequeña para que encaje perfecto

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(x => ComposeContent(x, listaParabrisas));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        private static void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Reporte de Inventario Físico").FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm}");
                    column.Item().Text("Sistema de Gestión de Parabrisas").FontSize(10).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private static void ComposeContent(IContainer container, IEnumerable<Parabrisa> listaParabrisas)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Spacing(5);

                column.Item().Table(table =>
                {
                    // 1. DEFINIMOS 7 COLUMNAS AHORA
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);  // ID
                        columns.RelativeColumn(3);   // Modelo
                        columns.RelativeColumn(2);   // Marca
                        columns.RelativeColumn(2);   // Tipo
                        columns.RelativeColumn(2);   // Ubicación (¡NUEVA COLUMNA!)
                        columns.RelativeColumn(1);   // Stock
                        columns.RelativeColumn(2);   // Precio
                    });

                    // 2. AGREGAMOS EL TÍTULO A LA CABECERA
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("ID").SemiBold().FontColor(Colors.White);
                        header.Cell().Element(CellStyle).Text("Tipo").SemiBold().FontColor(Colors.White);
                        header.Cell().Element(CellStyle).Text("Modelo").SemiBold().FontColor(Colors.White);
                        header.Cell().Element(CellStyle).Text("Marca").SemiBold().FontColor(Colors.White);
                        header.Cell().Element(CellStyle).Text("Ubicación").SemiBold().FontColor(Colors.White); // <-- AQUÍ
                        header.Cell().Element(CellStyle).Text("Stock").SemiBold().FontColor(Colors.White);
                        header.Cell().Element(CellStyle).Text("Precio").SemiBold().FontColor(Colors.White);

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black).Background(Colors.Blue.Darken2).AlignCenter();
                        }
                    });

                    // 3. LLENAMOS LOS DATOS DE LA UBICACIÓN
                    foreach (var item in listaParabrisas)
                    {
                        table.Cell().Element(BlockStyle).Text(item.Id.ToString());
                        table.Cell().Element(BlockStyle).Text(item.Tipo ?? "N/A");
                        table.Cell().Element(BlockStyle).Text(item.Marca ?? "N/A");
                        table.Cell().Element(BlockStyle).Text(item.Modelo ?? "N/A");

                        // Imprimimos la ubicación, si está vacía ponemos "Sin asignar"
                        table.Cell().Element(BlockStyle).Text(item.Ubicación ?? "Sin asignar");

                        table.Cell().Element(BlockStyle).Text(item.Stock.ToString());
                        table.Cell().Element(BlockStyle).Text($"${item.Precio:F2}");

                        static IContainer BlockStyle(IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).AlignCenter();
                        }
                    }
                });
            });
        }

        private static void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Página ");
                x.CurrentPageNumber();
                x.Span(" de ");
                x.TotalPages();
            });
        }
    }
}