using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace devShop
{
    public partial class ProductMaster : System.Web.UI.Page
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(typeof(ProductMaster));

        protected void Page_Load(object sender, EventArgs e)
        {
           

            if (!IsPostBack)
            {
               GeneratePieChart();
            }

        }

        private void GeneratePieChart()
        {
            // Sample data for the pie chart
            var chartData = new System.Collections.Generic.Dictionary<string, int>
            {
                { "Travel", 10 },
                { "Clothing", 15 },
                { "Decor", 25 },
                { "Fragrance", 10 },
                { "Footwear", 15 },
                { "Jewelery", 20 },
                { "Accessories", 5 },
            };

            int totalValue = 0;
            foreach (var item in chartData.Values)
            {
                totalValue += item;
            }

            // Image dimensions
            int width = 500;
            int height = 500;

            // Create a new Bitmap object
            using (Bitmap bitmap = new Bitmap(width, height))
            {
                try
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        // Set background color
                        graphics.FillRectangle(Brushes.LightGray, 0, 0, width, height);

                        // Define the pie chart area
                        Rectangle pieRect = new Rectangle(150, 150, 300, 300);

                        float startAngle = 0;
                        int i = 0;
                        Color[] colors = { Color.Red, Color.Green, Color.Blue, Color.Orange, Color.Purple, Color.Bisque, Color.BurlyWood }; // Define some colors

                        foreach (var entry in chartData)
                        {
                            float sweepAngle = (float)entry.Value / totalValue * 360;

                            // Draw the pie slice
                            using (Brush brush = new SolidBrush(colors[i % colors.Length]))
                            {
                                graphics.FillPie(brush, pieRect, startAngle, sweepAngle);
                            }

                            // Optional: Draw a border around the slice
                            graphics.DrawPie(Pens.Black, pieRect, startAngle, sweepAngle);

                            startAngle += sweepAngle;
                            i++;
                        }

                        // Add a simple legend
                        int legendX = 50;
                        int legendY = 50;
                        i = 0;
                        foreach (var entry in chartData)
                        {
                            using (Brush brush = new SolidBrush(colors[i % colors.Length]))
                            {
                                graphics.FillRectangle(brush, legendX, legendY, 20, 15);
                            }
                            graphics.DrawString($"{entry.Key} ({entry.Value})", new Font("Arial", 10), Brushes.Black, legendX + 25, legendY);
                            legendY += 20;
                            i++;
                        }

                        // Save the bitmap to a MemoryStream and then set it to the Image control
                        using (MemoryStream ms = new MemoryStream())
                        {
                            bitmap.Save(ms, ImageFormat.Png);
                            byte[] byteImage = ms.ToArray();
                            imgPieChart.ImageUrl = "data:image/png;base64," + Convert.ToBase64String(byteImage);
                            //imgPieChart.AlternateText= "An error occurred while loading categories: Object reference not set to an instance of an object.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error generating the product chart.", ex);
                }
            }


    }

        }
    }
