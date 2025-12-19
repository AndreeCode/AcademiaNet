using iTextSharp.text;
using iTextSharp.text.pdf;
using Academic.Data;
using Academic.Models;
using Microsoft.EntityFrameworkCore;

namespace Academic.Services;

public class PdfService
{
    private readonly AcademicContext _context;

    public PdfService(AcademicContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerarReporteMatriculadosAsync(int cicloId)
    {
        var ciclo = await _context.Ciclos.FindAsync(cicloId);
        if (ciclo == null) throw new ArgumentException("Ciclo no encontrado");

        var matriculas = await _context.Matriculas
            .Include(m => m.Alumno)
            .Include(m => m.Ciclo)
            .Where(m => m.CicloId == cicloId && m.EstadoPago == EstadoPago.Pagado)
            .OrderBy(m => m.Alumno!.Apellido)
            .ThenBy(m => m.Alumno!.Nombre)
            .ToListAsync();

        using var ms = new MemoryStream();
        var document = new Document(PageSize.A4);
        PdfWriter.GetInstance(document, ms);
        document.Open();

        // Título
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        var title = new Paragraph($"LISTA DE MATRICULADOS - {ciclo.Nombre}", titleFont)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 20
        };
        document.Add(title);

        // Información del ciclo
        var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
        document.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}", infoFont));
        document.Add(new Paragraph($"Total Matriculados: {matriculas.Count}", infoFont));
        document.Add(new Paragraph($"Vacantes Disponibles: {(ciclo.Vacantes > 0 ? (ciclo.Vacantes - matriculas.Count).ToString() : "Ilimitadas")}", infoFont));
        document.Add(new Paragraph(" "));

        // Tabla
        var table = new PdfPTable(6) { WidthPercentage = 100 };
        table.SetWidths(new float[] { 1, 3, 3, 2, 3, 2 });

        // Headers
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
        var headers = new[] { "#", "Apellidos", "Nombres", "DNI", "Email", "Fecha Pago" };
        foreach (var header in headers)
        {
            var cell = new PdfPCell(new Phrase(header, headerFont))
            {
                BackgroundColor = new BaseColor(211, 211, 211), // Light gray
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5
            };
            table.AddCell(cell);
        }

        // Datos
        var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
        var index = 1;
        foreach (var m in matriculas)
        {
            table.AddCell(new Phrase(index.ToString(), dataFont));
            table.AddCell(new Phrase(m.Alumno?.Apellido ?? "", dataFont));
            table.AddCell(new Phrase(m.Alumno?.Nombre ?? "", dataFont));
            table.AddCell(new Phrase(m.Alumno?.DNI ?? "", dataFont));
            table.AddCell(new Phrase(m.Alumno?.Email ?? "", dataFont));
            table.AddCell(new Phrase(m.FechaPago?.ToString("dd/MM/yyyy") ?? "", dataFont));
            index++;
        }

        document.Add(table);
        document.Close();

        return ms.ToArray();
    }

    public async Task<byte[]> GenerarReporteEstadisticasAsync(int cicloId)
    {
        var ciclo = await _context.Ciclos.FindAsync(cicloId);
        if (ciclo == null) throw new ArgumentException("Ciclo no encontrado");

        var totalMatriculas = await _context.Matriculas
            .Where(m => m.CicloId == cicloId)
            .CountAsync();

        var matriculasPagadas = await _context.Matriculas
            .Where(m => m.CicloId == cicloId && m.EstadoPago == EstadoPago.Pagado)
            .CountAsync();

        var matriculasPendientes = await _context.Matriculas
            .Where(m => m.CicloId == cicloId && m.EstadoPago == EstadoPago.Pendiente)
            .CountAsync();

        var totalRecaudado = await _context.Matriculas
            .Where(m => m.CicloId == cicloId && m.EstadoPago == EstadoPago.Pagado)
            .SumAsync(m => m.Monto);

        using var ms = new MemoryStream();
        var document = new Document(PageSize.A4);
        PdfWriter.GetInstance(document, ms);
        document.Open();

        // Título
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        var title = new Paragraph($"ESTADÍSTICAS - {ciclo.Nombre}", titleFont)
        {
            Alignment = Element.ALIGN_CENTER,
            SpacingAfter = 30
        };
        document.Add(title);

        // Información general
        var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
        var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);

        document.Add(new Paragraph($"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}", infoFont));
        document.Add(new Paragraph(" "));

        // Estadísticas
        document.Add(new Paragraph("RESUMEN DE MATRÍCULAS", boldFont) { SpacingBefore = 10, SpacingAfter = 10 });
        document.Add(new Paragraph($"Total de matrículas: {totalMatriculas}", infoFont));
        document.Add(new Paragraph($"Matrículas pagadas: {matriculasPagadas}", infoFont));
        document.Add(new Paragraph($"Matrículas pendientes: {matriculasPendientes}", infoFont));
        document.Add(new Paragraph($"Vacantes totales: {(ciclo.Vacantes > 0 ? ciclo.Vacantes.ToString() : "Ilimitadas")}", infoFont));
        document.Add(new Paragraph($"Vacantes disponibles: {(ciclo.Vacantes > 0 ? (ciclo.Vacantes - matriculasPagadas).ToString() : "Ilimitadas")}", infoFont));
        document.Add(new Paragraph(" "));

        document.Add(new Paragraph("INFORMACIÓN FINANCIERA", boldFont) { SpacingBefore = 10, SpacingAfter = 10 });
        document.Add(new Paragraph($"Monto por matrícula: S/ {ciclo.MontoMatricula:N2}", infoFont));
        document.Add(new Paragraph($"Total recaudado: S/ {totalRecaudado:N2}", infoFont));
        document.Add(new Paragraph($"Pendiente de pago: S/ {(matriculasPendientes * ciclo.MontoMatricula):N2}", infoFont));
        document.Add(new Paragraph(" "));

        document.Add(new Paragraph("INFORMACIÓN DEL CICLO", boldFont) { SpacingBefore = 10, SpacingAfter = 10 });
        document.Add(new Paragraph($"Modalidad: {ciclo.Modalidad}", infoFont));
        document.Add(new Paragraph($"Fecha inicio: {ciclo.FechaInicio:dd/MM/yyyy}", infoFont));
        document.Add(new Paragraph($"Fecha fin: {ciclo.FechaFin:dd/MM/yyyy}", infoFont));
        if (ciclo.MatriculaInicio.HasValue && ciclo.MatriculaFin.HasValue)
        {
            document.Add(new Paragraph($"Periodo de matrícula: {ciclo.MatriculaInicio.Value:dd/MM/yyyy} - {ciclo.MatriculaFin.Value:dd/MM/yyyy}", infoFont));
        }

        document.Close();
        return ms.ToArray();
    }
}
