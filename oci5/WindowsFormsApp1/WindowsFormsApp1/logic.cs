using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;



namespace oci_1
{
    class logic
    {
        
        public Emgu.CV.Image<Bgr, byte> openImg()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файлы изображений (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png)";
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла

            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = openFileDialog.FileName;
                return new Image<Bgr, byte>(fileName).Resize(640, 480, Inter.Linear);
            }
            return null;
        }

        public string openVideo()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var result = openFileDialog.ShowDialog(); 
            if (result == DialogResult.OK) 
            {
                string fileName = openFileDialog.FileName;
                return fileName;
            }
            return null;
        }

        public Emgu.CV.Image<Bgr, byte> makeMagick(Image<Bgr, byte> sourceImage, double cannyThreshold, double cannyThresholdLinking, int dilagate)
        {
            Image<Gray, byte> grayImage = sourceImage.Convert<Gray, byte>();

            var tempImage = grayImage.PyrDown();
            var destImage = tempImage.PyrUp();
            Image<Gray, byte> cannyEdges = destImage.Canny(cannyThreshold, cannyThresholdLinking);

            cannyEdges._Dilate(dilagate);

            var cannyEdgesBgr = cannyEdges.Convert<Bgr, byte>();
            var resultImage = sourceImage.Sub(cannyEdgesBgr); // попиксельное вычитание
                                                              //обход по каналам
            for (int channel = 0; channel < resultImage.NumberOfChannels; channel++)
                for (int x = 0; x < resultImage.Width; x++)
                    for (int y = 0; y < resultImage.Height; y++) // обход по пискелям
                    {
                        // получение цвета пикселя
                        byte color = resultImage.Data[y, x, channel];
                        if (color <= 50)
                            color = 0;
                        //else if (color <= 100)
                        //    color = 25;
                        else if (color <= 150)
                            color = 180;
                        else if (color <= 200)
                            color = 210;
                        else
                            color = 255;
                        resultImage.Data[y, x, channel] = color; // изменение цвета пикселя
                    }

            return resultImage;
        }

        public Emgu.CV.Image<Bgr, byte> chanelsChange(Image<Bgr, byte> sourceImage, int type)
        {
            var channel = sourceImage.Split()[type];

            Image<Bgr, byte> destImage = sourceImage.CopyBlank();

            VectorOfMat vm = new VectorOfMat();

            if (type == 0)
            {
                vm.Push(channel);
                vm.Push(channel.CopyBlank());
                vm.Push(channel.CopyBlank());
            }
            else if (type == 1)
            {
                vm.Push(channel.CopyBlank());
                vm.Push(channel.CopyBlank());
                vm.Push(channel);
            }
            else if (type == 2)
            {
                vm.Push(channel.CopyBlank());
                vm.Push(channel);
                vm.Push(channel.CopyBlank());
            }

            CvInvoke.Merge(vm, destImage);

            return destImage.Resize(640, 480, Inter.Linear);
        }

        public Emgu.CV.Image<Bgr, byte> contrast(Image<Bgr, byte> sourceImage, double value)
        {
            var contrastImage = sourceImage.Copy();

            for (int ch = 0; ch < 3; ch++)
                for (int y = 0; y < contrastImage.Height; y++)
                    for (int x = 0; x < contrastImage.Width; x++)
                    {
                        if ((contrastImage.Data[y, x, ch] * value) > 255)
                            contrastImage.Data[y, x, ch] = 255;
                        else
                            contrastImage.Data[y, x, ch] = Convert.ToByte(contrastImage.Data[y, x, ch] * value);
                    }

            return contrastImage;
        }

        public Emgu.CV.Image<Bgr, byte> makeGray(Image<Bgr, byte> sourceImage)
        {
            var grayImage = sourceImage.Copy();

            for (int x = 0; x < grayImage.Width; x++)
            {
                for (int y = 0; y < grayImage.Height; y++)
                {
                    for (int c = 0; c <= 2; c++)
                    {
                        grayImage.Data[y, x, c] = Convert.ToByte(0.1 * grayImage.Data[y, x, 0] + 0.6 * grayImage.Data[y, x, 1] + 0.3 * grayImage.Data[y, x, 2]);
                    }
                }
            }
            return grayImage;
        }

        public Image<Bgr, byte> makeSepia(Image<Bgr, byte> sourceImage)
        {
            var resultImage = new Image<Bgr, byte>(sourceImage.Size);
            for (int x = 0; x < sourceImage.Width; x++)
                for (int y = 0; y < sourceImage.Height; y++)
                {
                    var red = sourceImage[y, x].Red;
                    var green = sourceImage[y, x].Green;
                    var blue = sourceImage[y, x].Blue;

                    var color = red * 0.393 + green * 0.769 + blue * 0.189;
                    if (color > 255) color = 255;
                    resultImage.Data[y, x, 2] = Convert.ToByte(color);

                    color = red * 0.349 + green * 0.686 + blue * 0.168;
                    if (color > 255) color = 255;
                    resultImage.Data[y, x, 1] = Convert.ToByte(color);

                    color = red * 0.272 + green * 0.534 + blue * 0.131;
                    if (color > 255) color = 255;
                    resultImage.Data[y, x, 0] = Convert.ToByte(color);
                }
            return resultImage;
        }

        public Image<Hsv, byte> makeHsv(Image<Bgr, byte> sourceImage)
         {
                var hsvImage = sourceImage.Convert<Hsv, byte>();
                return hsvImage;
         }

        public Image<Hsv, byte> remakeHSV(Image<Bgr, byte> sourceImage, int chn, int val)
        {
            var hsvImage = makeHsv(sourceImage);
            for (int i = 0; i < hsvImage.Width; i++)
            {
                for (int j = 0; j < hsvImage.Height; j++)
                {
                    if (hsvImage.Data[j, i, chn] + val > 255)
                    {
                        hsvImage.Data[j, i, chn] = 255;
                    }
                    else
                    {
                        hsvImage.Data[j, i, chn] += Convert.ToByte(val);
                    }
                }
            }
            return hsvImage;
        }

          
        public Image<Bgr, byte> Plus(Image<Bgr, byte> image, Image<Bgr, byte> secimage, int sum, double cf1, double cf2)
        {
            var brightimage = image.Copy();
                for (int x = 0; x < brightimage.Width; x++)
                {
                    for (int y = 0; y < brightimage.Height; y++)
                    {
                        for (int c = 0; c <= 2; c++)
                        {
                            if (secimage != null)
                            {
                                if ((image.Data[y, x, c] * cf1 + secimage.Data[y, x, c] * cf2) > 255)
                                {
                                    brightimage.Data[y, x, c] = 255;
                                }
                                else
                                {
                                    brightimage.Data[y, x, c] = Convert.ToByte(image.Data[y, x, c] * cf1 + secimage.Data[y, x, c] * cf2);
                                }
                            }
                            else
                            {
                                if ((brightimage.Data[y, x, c] + sum) > 255)
                                {
                                    brightimage.Data[y, x, c] = 255;
                                }
                                else
                                {
                                    brightimage.Data[y, x, c] += Convert.ToByte(sum);
                                }
                            }
                        }
                    }
                }
            return brightimage;
        }
           
        

        public Image<Bgr, byte> Minus(Image<Bgr, byte> image, Image<Bgr, byte> secimage, int sum, double cf1, double cf2)
        {
            var brightimage = image.Copy();
            for (int x = 0; x < brightimage.Width; x++)
            {
                for (int y = 0; y < brightimage.Height; y++)
                {
                    for (int c = 0; c <= 2; c++)
                    {
                        if (secimage != null)
                        {
                            if (Convert.ToInt16(image.Data[y, x, c] * cf1 - secimage.Data[y, x, c] * cf2) < 0)
                            {
                                brightimage.Data[y, x, c] = 0;
                            }
                            else
                            {
                                brightimage.Data[y, x, c] = Convert.ToByte(image.Data[y, x, c] * cf1 - secimage.Data[y, x, c] * cf2);
                            }
                        }
                        else
                        {
                            if ((Convert.ToInt32(brightimage.Data[y, x, c]) + sum) < 0)
                            {
                                brightimage.Data[y, x, c] = 0;
                            }
                            else
                            {
                                brightimage.Data[y, x, c] = Convert.ToByte(Convert.ToInt16(brightimage.Data[y, x, c]) + sum);
                            }
                        }
                    }
                }
            }
            return brightimage;
        }

        public Image<Bgr, byte> Peresech(Image<Bgr, byte> image, Image<Bgr, byte> secimage, double sum, double cf1, double cf2)
        {
            var contrastimage = image.Copy();
            for (int x = 0; x < contrastimage.Width; x++)
            {
                for (int y = 0; y < contrastimage.Height; y++)
                {
                    for (int c = 0; c <= 2; c++)
                    {
                        if (secimage != null)
                        {
                            if (Convert.ToDouble((image.Data[y, x, c] * cf1 / 10) * (secimage.Data[y, x, c] * cf2 / 10)) > 255)
                            {
                                contrastimage.Data[y, x, c] = 255;
                            }
                            else
                            {
                                contrastimage.Data[y, x, c] = Convert.ToByte((image.Data[y, x, c] * cf1 / 10) * (secimage.Data[y, x, c] * cf2 / 10));
                            }
                        }
                        else
                        {
                            if ((contrastimage.Data[y, x, c] * Convert.ToDouble(sum / 10)) > 255)
                            {
                                contrastimage.Data[y, x, c] = 255;
                            }
                            else
                            {
                                contrastimage.Data[y, x, c] = Convert.ToByte(contrastimage.Data[y, x, c] * Convert.ToDouble(sum / 10));
                            }
                        }
                    }
                }
            }
            return contrastimage;
        }
    
          

        public Image<Bgr, byte> makeOperate(Image<Bgr, byte> sourceImage, int type)
        {
            var resultImage = new Image<Bgr, byte>(sourceImage.Size);
            switch (type)
            {
                case 0:
                    {
                        return Plus(sourceImage,openImg(),1,0.5,0.5);
                        
                    }
                case 1:
                    {
                        return Minus(sourceImage, openImg(), 1, 0.5, 0.5);
                       
                    }
                case 2:
                    {
                        return Peresech(sourceImage, openImg(), 1, 0.5, 0.5);
                       
                    }
            }
            return null;

        }

        public Image<Bgr, byte> makeBlur(Image<Bgr, byte> sourceImage)
        {
            var blurimg = sourceImage;
            var finalimage = blurimg.CopyBlank();

            for (int i = 1; i <= blurimg.Width-2; i++)
            {
                for (int j = 1; j <= blurimg.Height-2; j++)
                {
                    for (int c = 0; c <= 2; c++)
                    {
                        List<byte> blurl = new List<byte>();
                        for (int x = -1; x < 2; x++)
                        {
                            for (int y = -1; y < 2; y++)
                            {
                                blurl.Add(blurimg.Data[j + x, i + y, c]);
                            }
                        }
                        blurl.Sort();
                        finalimage.Data[j, i, c] = blurl[4];
                    }
                }
            }
            return finalimage;
        }

        public Image<Gray, byte> Sharpen(Image<Bgr, byte> sourceImage, int[,] wnd)
        {
        

            var bl = makeGray(sourceImage);

            var resultImage = new Image<Gray, byte>(bl.Size);
            for (int x = 1; x < bl.Width - 1; x++)
                for (int y = 1; y < bl.Height - 1; y++)
                {
                    double cB = 0;
                    for (int i = -1; i <= 1; i++)
                        for (int j = -1; j <= 1; j++)
                        {
                            cB += sourceImage.Data[y + i, x + j, 0] * wnd[i + 1, j + 1];
                        }
                    if (cB > 254)
                        cB = 255;
                    if (cB < 0)
                        cB = 0;
                    resultImage.Data[y, x, 0] = Convert.ToByte(cB);

                }
            return resultImage;
        }

        public Image<Bgr,byte> makeAcva (Image<Bgr,byte> sourceImage)
        {
            var temp = makeOperate(sourceImage, 0);
            var temp1 = makeBlur(temp);
            return temp1;

        }

        public Image<Gray, byte> makeGrayde(Image<Bgr, byte> sourceImage)
        {
            var edges = makeGray(sourceImage);
            Image<Gray, byte> grayedges = edges.Convert<Gray, byte>();
            grayedges = grayedges.ThresholdAdaptive(new Gray(255), AdaptiveThresholdType.MeanC, ThresholdType.Binary, 3, new Gray(0.03));
            return grayedges;
        }

        public Image<Bgr, byte> makeCartoon(Image<Bgr, byte> sourceImage)
        {
            var carimg = makeBlur(sourceImage);
            Image<Gray, byte> grayedge = makeGrayde(carimg);
            for (int i = 0; i < grayedge.Width; i++)
            {
                for (int j = 0; j < grayedge.Height; j++)
                {
                    if (grayedge.Data[j, i, 0] > 0)
                    {

                    }
                    else
                    {
                        carimg.Data[j, i, 0] = 0;
                        carimg.Data[j, i, 1] = 0;
                        carimg.Data[j, i, 2] = 0;
                    }
                }
            }
            return carimg;
        }

        public Image<Bgr, byte> matrixFiltr(Image<Bgr, byte> sourceImage, int[,] mat)
        {
            var sharpimage = sourceImage.Copy();
            var finalimage = sharpimage.CopyBlank();
            for (int i = 1; i < sharpimage.Width-2; i++)
            {
                for (int j = 1; j < sharpimage.Height-2; j++)
                {
                    for (int c = 0; c <= 2; c++)
                    {
                        int result = 0;
                        for (int x = -1; x < 2; x++)
                        {
                            for (int y = -1; y < 2; y++)
                            {
                                result += sharpimage.Data[j + y, i + x, c] * mat[x + 1, y + 1];
                            }
                        }

                        if (result > 255) result = 255; else if (result < 0) result = 0;
                        finalimage.Data[j, i, c] = Convert.ToByte(result);
                    }
                }
            }
            return finalimage;
        }


        public Image<Bgr,byte> makeBILINEARSH(Image<Bgr,byte> sourceImage, double k)
        {
            Image<Bgr, byte> scaledImg = new Image<Bgr, byte>((int)(sourceImage.Width * k), (int)(sourceImage.Height * k));
            for (int i = 0; i < scaledImg.Width - 1; i++)

                for (int j = 0; j < scaledImg.Height - 1; j++)
                {
                    double I = (i / k);
                    double J = (j / k);

                    double baseI = Math.Floor(I);
                    double baseJ = Math.Floor(J);

                    if (baseI >= sourceImage.Width - 1) continue;
                    if (baseJ >= sourceImage.Height - 1) continue;

                    double rI = I - baseI;
                    double rJ = J - baseJ;

                    double irI = 1 - rI;
                    double irJ = 1 - rJ;

                    Bgr c1 = new Bgr();
                    c1.Blue = sourceImage.Data[(int)baseJ, (int)baseI, 0] * irI + sourceImage.Data[(int)baseJ, (int)(baseI + 1), 0] * rI;
                    c1.Green = sourceImage.Data[(int)baseJ, (int)baseI, 1] * irI + sourceImage.Data[(int)baseJ, (int)(baseI + 1), 1] * rI;
                    c1.Red = sourceImage.Data[(int)baseJ, (int)baseI, 2] * irI + sourceImage.Data[(int)baseJ, (int)(baseI + 1), 2] * rI;

                    Bgr c2 = new Bgr();
                    c2.Blue = sourceImage.Data[(int)(baseJ + 1), (int)baseI, 0] * irI + sourceImage.Data[(int)(baseJ + 1), (int)(baseI + 1), 0] * rI;
                    c2.Green = sourceImage.Data[(int)(baseJ + 1), (int)baseI, 1] * irI + sourceImage.Data[(int)(baseJ + 1), (int)(baseI + 1), 1] * rI;
                    c2.Red = sourceImage.Data[(int)(baseJ + 1), (int)baseI, 2] * irI + sourceImage.Data[(int)(baseJ + 1), (int)(baseI + 1), 2] * rI;

                    Bgr c = new Bgr();
                    c.Blue = c1.Blue * irJ + c2.Blue * rJ;
                    c.Green = c1.Green * irJ + c2.Green * rJ;
                    c.Red = c1.Red * irJ + c2.Red * rJ;

                    scaledImg[j, i] = c;

                }
            return scaledImg;
        }

        public PointF[] pts { get; set; } = new PointF[4];
        public Image<Bgr, byte> Homography(Image<Bgr, byte> copyImage)
        {
            var destPoints = new PointF[]
            {
                new PointF(0, 0),
                new PointF(0, copyImage.Height - 1),
                new PointF(copyImage.Width - 1, copyImage.Height - 1),
                new PointF(copyImage.Width - 1, 0)
            };

            var homographyMatrix = CvInvoke.GetPerspectiveTransform(pts, destPoints);
            var destImage = new Image<Bgr, byte>(copyImage.Size);
            CvInvoke.WarpPerspective(copyImage, destImage, homographyMatrix, destImage.Size);

            return destImage;
        }

        public Image<Bgr, byte> Shearing(Image<Bgr, byte> sourceImage, double shift)
        {
            Image<Bgr, byte> shearingImg = new Image<Bgr, byte>((int)(sourceImage.Width + sourceImage.Width * shift) + 1, (int)(sourceImage.Height));
            for (int i = 0; i < sourceImage.Width - 1; i++)
                for (int j = 0; j < sourceImage.Height - 1; j++)
                {
                    int newX = (int)(i + shift * (sourceImage.Height - j));
                    int newY = (int)(j);
                    shearingImg[newY, newX] = sourceImage[j, i];
                }

            return shearingImg;
        }
        public Image<Bgr, byte> Rotation(Image<Bgr, byte> sourceImage, double angle)
        {
            Image<Bgr, byte> scaledImage = new Image<Bgr, byte>((int)sourceImage.Width, (int)sourceImage.Height);
            double newX = 0;
            double newY = 0;

            double angleRadians = angle * Math.PI / 180d;

            for (int x = 0; x < scaledImage.Width; x++)
                for (int y = 0; y < scaledImage.Height; y++)
                {
                    newX = Math.Abs(Math.Cos(angleRadians) * (x - scaledImage.Width / 2) - Math.Sin(angleRadians) * (y - scaledImage.Height / 2) + scaledImage.Width / 2);
                    newY = Math.Abs(Math.Sin(angleRadians) * (x - scaledImage.Width / 2) + Math.Cos(angleRadians) * (y - scaledImage.Height / 2) + scaledImage.Height / 2);

                    if (newX >= sourceImage.Width || newY >= sourceImage.Height) continue;

                    scaledImage[y, x] = sourceImage[(int)newY, (int)newX];
                }
            //Smooth(sourceImage, scaledImage, a, newX, newY);

            return scaledImage;
        }
        public Image<Bgr, byte> Reflection(Image<Bgr, byte> sourceImage,double qX, double qY)
        {
            Image<Bgr, byte> scaledImage = new Image<Bgr, byte>((int)sourceImage.Width, (int)sourceImage.Height);

            double newX = 0;
            double newY = 0;

            for (int x = 0; x < scaledImage.Width; x++)
                for (int y = 0; y < scaledImage.Height; y++)
                {
                    if (qX == 1 && qY == 1)
                    {
                        newX = x;
                        newY = y;
                    }
                    else if (qX == -1 && qY == -1)
                    {
                        newX = (x * (qX) + sourceImage.Width);
                        newY = (y * (qY) + sourceImage.Height);
                    }
                    else if (qX == 1 && qY == -1)
                    {
                        newX = (x * (qX));
                        newY = (y * (qY) + sourceImage.Height);
                    }
                    else if (qX == -1 && qY == 1)
                    {
                        newX = (x * (qX) + sourceImage.Width);
                        newY = (y * (qY));
                    }

                    if (newX >= sourceImage.Width || newY >= sourceImage.Height) continue;

                    scaledImage[y, x] = sourceImage[(int)newY, (int)newX];
                }

            return scaledImage;
        }

        public Image<Gray, byte> findROI(Image<Bgr, byte> image, int n1, int n2)
        {
            var grayImage = image.Convert<Gray, byte>();
            var bluredImage = grayImage.SmoothGaussian(n1);
            var color = new Gray(255);
            var binarizedImage = bluredImage.ThresholdBinary(new Gray(n2), color);

            return binarizedImage;
        }
        public Image<Bgr, byte> findEdges(Image<Bgr, byte> image, int n1, int n2, int thicc)
        {
            Image<Gray, byte> binarizedImage = findROI(image, n1, n2);
            var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(
                binarizedImage,
                contours,
                null,
                RetrType.List,
                ChainApproxMethod.ChainApproxSimple);

            var contoursImage = image.Copy();
            for (int i = 0; i < contours.Size; i++)
            {
                var points = contours[i].ToArray();
                contoursImage.Draw(points, new Bgr(Color.GreenYellow), thicc);
            }

            return contoursImage;
        }

        public Image<Bgr, byte> findTriangles(Image<Bgr, byte> image, int n1, int n2, int thicc, int tresh)
        {
            Image<Gray, byte> binarizedImage = findROI(image, n1, n2);
            var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(
                binarizedImage,
                contours,
                null,
                RetrType.List,
                ChainApproxMethod.ChainApproxSimple);

            var contoursImage = image.Copy();

            var approxContour = new VectorOfPoint();

            for (int i = 0; i < contours.Size; i++)
            {
                CvInvoke.ApproxPolyDP(
                    contours[i],
                    approxContour,
                    CvInvoke.ArcLength(contours[i], true) * 0.05,
                    true);
                if (CvInvoke.ContourArea(approxContour, false) > tresh)
                {

                    if (approxContour.Size == 3)
                    {
                        var points = approxContour.ToArray();
                        contoursImage.Draw(new Triangle2DF(points[0], points[1], points[2]),
                        new Bgr(Color.GreenYellow), thicc);
                    }
                }
            }

            return contoursImage;
        }

        public Image<Bgr, byte> findRectangles(Image<Bgr, byte> image, int n1, int n2, int thicc, int tresh, int delta)
        {
            Image<Gray, byte> binarizedImage = findROI(image, n1, n2);
            var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(
                binarizedImage,
                contours,
                null,
                RetrType.List,
                ChainApproxMethod.ChainApproxSimple);

            var contoursImage = image.Copy();

            var approxContour = new VectorOfPoint();

            for (int i = 0; i < contours.Size; i++)
            {
                CvInvoke.ApproxPolyDP(
                    contours[i],
                    approxContour,
                    CvInvoke.ArcLength(contours[i], true) * 0.05,
                    true);

                if (CvInvoke.ContourArea(approxContour, false) > tresh)
                {
                    var points = approxContour.ToArray();
                    if (rectangleCheck(points, delta) == true)
                    {
                        contoursImage.Draw(CvInvoke.MinAreaRect(approxContour), new Bgr(Color.GreenYellow), thicc);
                    }
                }
            }
            return contoursImage;
        }

        private bool rectangleCheck(Point[] points, int delta)
        {
            LineSegment2D[] edges = PointCollection.PolyLine(points, true);
            for (int i = 0; i < edges.Length; i++)
            {
                double angle = Math.Abs(edges[(i + 1) %
                edges.Length].GetExteriorAngleDegree(edges[i]));
                if (angle < 90 - delta || angle > 90 + delta)
                {
                    return false;
                }
            }
            return true;
        }

        public Image<Bgr, byte> findCircles(Image<Bgr, byte> image, int k1, int k2, int k3, int k4)
        {
            var grayImage = image.Convert<Gray, byte>();
            var bluredImage = grayImage.SmoothGaussian(9);

            List<CircleF> circles = new List<CircleF>(CvInvoke.HoughCircles(bluredImage,
                 HoughModes.Gradient,
                 1.0,
                 k1,
                 100,
                 k2,
                 k3,
                 k4));

            var resultImage = image.Copy();
            foreach (CircleF circle in circles)
            {
                resultImage.Draw(circle, new Bgr(Color.Blue), 2);
            }

            return resultImage.Resize(640, 480, Inter.Linear);
        }

        public Image<Gray, byte> monohromImage(Image<Bgr, byte> sourceImage, int k1)
        {
            var grayImage = sourceImage.Convert<Gray, byte>();
            Image<Gray, byte> binarizedImage;
            binarizedImage = grayImage.ThresholdBinaryInv(new Gray(k1), new Gray(255));
            return binarizedImage;
        }

        public Image<Gray, byte> dilitImg(Image<Bgr, byte> image, int k1, int k3)
        {
            var binarizedImage = monohromImage(image, k1);

            var dilatedImage = binarizedImage.Dilate(k3);

            return dilatedImage;
        }

        public Image<Bgr, byte> findROIbutBETTER(Image<Bgr, byte> image, int k1, int k3, int k4, List<Rectangle> rois)
        {
            var dilatedImage = dilitImg(image, k1, k3);

            var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(dilatedImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            var copy = image.Copy();
            for (int i = 0; i < contours.Size; i++)
            {
                if (CvInvoke.ContourArea(contours[i], false) > (k4 * 10))
                {
                    Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                    rois.Add(rect);

                    copy.Draw(rect, new Bgr(Color.Blue), 1);
                }
            }

            return copy;
        }


        public Image<Bgr,byte> DetectFace(Image<Bgr, byte> sourceImage, Image<Bgr,byte> second)
        {
           
            using (CascadeClassifier cascadeClassifier = new CascadeClassifier("D:\\haarcascade_frontalface_default.xml"))
            {
                var grayImage = sourceImage.Convert<Gray, byte>();
                // обнаружение лиц
                Rectangle[] facesDetected = cascadeClassifier.DetectMultiScale(grayImage, 1.1, 10,
                new Size(20, 20));
                // создание копии исходного изображения
                var copy = sourceImage.Copy();

                

                foreach (Rectangle face in facesDetected)
                {

                    var temp = second.Copy().Resize(face.Width, face.Height,Inter.Nearest);
                    second.Resize(face.Width, face.Height, Inter.Nearest);
                    copy.ROI = face;
                    // copy.Draw(face, new Bgr(Color.Yellow), 2);

                   
                    CvInvoke.cvCopy(temp, copy, new IntPtr()); // temp - картинка для наложение copy - куда накладываем new IntPtr - заглушка ебаная
                }
                copy.ROI = Rectangle.Empty;
                return copy;  
            }


        }


    }
  
}
