using SkiaSharp;

public class MapRendererService
{
    private readonly List<State> _states;

    public MapRendererService(List<State> states)
    {
        _states = states;
    }

    public void RenderMap(string outputPath)
    {
        const int width = 1920;
        const int height = 1080;
        const int pad = 20;

        var bitmap = new SKBitmap(width, height);
        var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.White);

        var projected = new Dictionary<State, List<List<(double x, double y)>>>(_states.Count);

        foreach (var state in _states)
        {
            var rings = new List<List<(double, double)>>();

            foreach (var ring in state.Polygons)
            {
                var projectedRing = new List<(double, double)>();

                foreach (var p in ring)
                {
                    double lat = p.Latitude * Math.PI / 180.0;
                    double lon = p.Longitude * Math.PI / 180.0;

                    double theta = ProectionAlgorithm.n * (lon - ProectionAlgorithm.l0);

                    double rho =
                        Math.Sqrt(ProectionAlgorithm.C - 2 * ProectionAlgorithm.n * Math.Sin(lat))
                        / ProectionAlgorithm.n;

                    double x = rho * Math.Sin(theta);
                    double y = ProectionAlgorithm.p0 - rho * Math.Cos(theta);

                    projectedRing.Add((x, y));
                }

                rings.Add(projectedRing);
            }

            projected[state] = rings;
        }

        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;

        foreach (var state in _states)
        {
            if (state.Code == "AK" || state.Code == "HI" || state.Code == "PR")
                continue;

            foreach (var ring in projected[state])
            {
                foreach (var (x, y) in ring)
                {
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }
        }

        double extraY = (maxY - minY) * (100.0 / height);
        minY -= extraY;

        var fillPaint = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = true };

        var strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 1,
            IsAntialias = true,
        };

        foreach (var state in _states)
        {
            if (state.Code == "AK" || state.Code == "HI" || state.Code == "PR")
                continue;

            fillPaint.Color = state.Color;

            foreach (var ring in projected[state])
            {
                var path = new SKPath();

                for (int i = 0; i < ring.Count; i++)
                {
                    var (x, y) = ring[i];

                    var (px, py) = ProectionAlgorithm.CalculatingPixels(
                        pad,
                        width,
                        height,
                        x,
                        y,
                        minX,
                        minY,
                        maxX,
                        maxY
                    );

                    if (i == 0)
                        path.MoveTo((float)px, (float)py);
                    else
                        path.LineTo((float)px, (float)py);
                }

                path.Close();

                canvas.DrawPath(path, fillPaint);
                canvas.DrawPath(path, strokePaint);
            }
        }

        DrawStateLabels(canvas, width, height, minX, minY, maxX, maxY);

        DrawAlaska(canvas, projected, width, height);
        DrawHawaii(canvas, projected, width, height);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(outputPath);

        data.SaveTo(stream);
    }

    private void DrawAlaska(
        SKCanvas canvas,
        Dictionary<State, List<List<(double x, double y)>>> projected,
        int width,
        int height
    )
    {
        var state = projected.Keys.FirstOrDefault(s => s.Code == "AK");
        if (state == null)
            return;

        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;

        foreach (var ring in projected[state])
        {
            foreach (var (x, y) in ring)
            {
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }

        double scaleX = 450.0 / (maxX - minX);
        double scaleY = 300.0 / (maxY - minY);
        double scale = Math.Max(scaleX, scaleY) * 2;

        double alaskaHeight = (maxY - minY) * scale;

        float offsetX = 50;
        float offsetY = (float)(height - alaskaHeight - 80);

        var fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = state.Color,
            IsAntialias = true,
        };

        var strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 1,
            IsAntialias = true,
        };

        foreach (var ring in projected[state])
        {
            var path = new SKPath();

            for (int i = 0; i < ring.Count; i++)
            {
                var (x, y) = ring[i];

                float px = (float)((x - minX) * scale) + offsetX;
                float py = (float)((maxY - y) * scale) + offsetY;

                if (i == 0)
                    path.MoveTo(px, py);
                else
                    path.LineTo(px, py);
            }

            path.Close();

            canvas.DrawPath(path, fillPaint);
            canvas.DrawPath(path, strokePaint);
        }

        float centerX = (float)((maxX - minX) * scale / 2);
        float centerY = (float)((maxY - minY) * scale / 2);

        var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };

        var font = new SKFont { Size = 22 };

        canvas.DrawText("AK", centerX - 120, centerY + 50, SKTextAlign.Center, font, paint);
    }

    private void DrawHawaii(
        SKCanvas canvas,
        Dictionary<State, List<List<(double x, double y)>>> projected,
        int width,
        int height
    )
    {
        var state = projected.Keys.FirstOrDefault(s => s.Code == "HI");
        if (state == null)
            return;

        double minX = double.MaxValue;
        double minY = double.MaxValue;
        double maxX = double.MinValue;
        double maxY = double.MinValue;

        foreach (var ring in projected[state])
        {
            foreach (var (x, y) in ring)
            {
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }

        double scaleX = 200.0 / (maxX - minX);
        double scaleY = 120.0 / (maxY - minY);
        double scale = Math.Min(scaleX, scaleY) * 1.5;

        double hawaiiHeight = (maxY - minY) * scale;

        float offsetX = 550;
        float offsetY = (float)(height - hawaiiHeight - 70);

        var fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = state.Color,
            IsAntialias = true,
        };

        var strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 1,
            IsAntialias = true,
        };

        foreach (var ring in projected[state])
        {
            var path = new SKPath();

            for (int i = 0; i < ring.Count; i++)
            {
                var (x, y) = ring[i];

                float px = (float)((x - minX) * scale) + offsetX;
                float py = (float)((maxY - y) * scale) + offsetY;

                if (i == 0)
                    path.MoveTo(px, py);
                else
                    path.LineTo(px, py);
            }

            path.Close();

            canvas.DrawPath(path, fillPaint);
            canvas.DrawPath(path, strokePaint);
        }

        var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };

        var font = new SKFont { Size = 22 };

        canvas.DrawText("HI", offsetX + 60, offsetY + 50, SKTextAlign.Center, font, paint);
    }

    private void DrawStateLabels(
        SKCanvas canvas,
        int width,
        int height,
        double minX,
        double minY,
        double maxX,
        double maxY
    )
    {
        const int pad = 20;

        var paint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
        };

        foreach (var state in _states)
        {
            if (state.Code == "AK" || state.Code == "HI" || state.Code == "PR")
                continue;

            double lat = state.Center.Latitude * Math.PI / 180.0;
            double lon = state.Center.Longitude * Math.PI / 180.0;

            double theta = ProectionAlgorithm.n * (lon - ProectionAlgorithm.l0);

            double rho =
                Math.Sqrt(ProectionAlgorithm.C - 2 * ProectionAlgorithm.n * Math.Sin(lat))
                / ProectionAlgorithm.n;

            double x = rho * Math.Sin(theta);
            double y = ProectionAlgorithm.p0 - rho * Math.Cos(theta);

            var (px, py) = ProectionAlgorithm.CalculatingPixels(
                pad,
                width,
                height,
                x,
                y,
                minX,
                minY,
                maxX,
                maxY
            );

            var font = new SKFont { Size = 18 };

            canvas.DrawText(state.Code, (float)px, (float)py, SKTextAlign.Center, font, paint);
        }
    }
}
