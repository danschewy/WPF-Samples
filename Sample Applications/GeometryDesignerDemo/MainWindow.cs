// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.Xml.Linq;

namespace GeometryDesignerDemo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnUiReady(object sender, EventArgs e)
        {
            SizeChanged += Window1_SizeChanged;
        }

        private void Window1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            XAMLPane.BeginAnimation(Canvas.TopProperty, null);
            if (_isShow)
            {
                XAMLPane.SetValue(Canvas.TopProperty, e.NewSize.Height - 70);
            }
            else
            {
                XAMLPane.SetValue(Canvas.TopProperty, e.NewSize.Height);
            }
            XAMLPane.Width = e.NewSize.Width - 205;
        }

        private void OnCaptureCanvas(object sender, RoutedEventArgs e)
        {
            action_Done();
            if (_selectedIndex != null) { ((Path)DrawingPane.Children[(int)_selectedIndex]).Stroke = Brushes.White; _selectedIndex = null; }

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)DesignerPane.RenderSize.Width,
                (int)DesignerPane.RenderSize.Height, 96d, 96d, PixelFormats.Default);
            rtb.Render(DesignerPane);

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var fs = System.IO.File.OpenWrite("C:/Users/Daniel/Desktop/latest_canvas_capture.png"))
            {
                pngEncoder.Save(fs);
            }
        }

        public abstract class GeometryBase
        {
            protected GeometryBase()
            {
            }

            public abstract void Parse();
            public abstract Geometry CreateGeometry();

            protected double DoubleParser(string s) => double.Parse(s);

            protected Point PointParser(string o)
            {
                try
                {
                    return Point.Parse(o);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    throw new ApplicationException(
                        "Error: please enter two numeric values separated  by a comma or a space; for example, 10,30.");
                }
            }

            protected Size SizeParser(string o)
            {
                var retval = new Size();

                var sizeString = o.Split(' ', ',', ';');
                if (sizeString.Length == 0 || sizeString.Length != 2)
                    throw new ApplicationException(
                        "Error: a size should contain two double that seperated by a space or ',' or ';'");

                try
                {
                    var d1 = Convert.ToDouble(sizeString[0], CultureInfo.InvariantCulture);
                    var d2 = Convert.ToDouble(sizeString[1], CultureInfo.InvariantCulture);
                    retval.Width = d1;
                    retval.Height = d2;
                }
                catch (Exception)
                {
                    throw new ApplicationException("Error: please enter only two numeric values into the field");
                }

                return retval;
            }

            #region Data member

            protected FrameworkElement ParentPane;
            public ArrayList ControlPoints;
            public string GeometryType;

            #endregion
        }

        public class LineG : GeometryBase
        {
            public LineG()
                : base()
            {
                ControlPoints = new ArrayList();
                GeometryType = "Line";
                Parse();
            }

            public override void Parse()
            {
                _startPoint = new Point(300 + (25 * _lineCount), 300);
                _endPoint = new Point(300 + (25 * _lineCount), 200);
                ControlPoints.Add(_startPoint);
                ControlPoints.Add(_endPoint);
            }

            public override Geometry CreateGeometry()
            {
                var lg = new LineGeometry(_startPoint, _endPoint);
                return lg;
            }

            #region Data members

            private Point _startPoint;
            private Point _endPoint;

            #endregion
        }

        public class EllipseG : GeometryBase
        {
            public EllipseG() : base()
            {
                ControlPoints = new ArrayList();
                GeometryType = "Ellipse";
                Parse();
            }

            public override void Parse()
            {
                _center = new Point(300.0 + (25 * _elliipseCount), 300.0);
                _radiusx = 100;
                _radiusy = 50;

                //Center point
                ControlPoints.Add(_center);

                //TopLeft
                ControlPoints.Add(new Point(_center.X - _radiusx, _center.Y));

                //TopMiddle
                ControlPoints.Add(new Point(_center.X, _center.Y - _radiusy));

                //TopRight
                ControlPoints.Add(new Point(_center.X + _radiusx, _center.Y));

                //BottomMiddle
                ControlPoints.Add(new Point(_center.X, _center.Y + _radiusy));
            }

            public override Geometry CreateGeometry()
            {
                var retval = new EllipseGeometry(_center, _radiusx, _radiusy);
                return retval;
            }

            #region Data Members

            private Point _center;
            private double _radiusx;
            private double _radiusy;

            #endregion
        }

        // ---------------------------------------------------------------------------------

        #region private action functions

        //If mouse leaves the DesignerPane, hide all of the control points

        private void path_MouseEnter(object sender, MouseEventArgs e)
        {
            /*var pathId = ((Path) sender).Name;

            //Search and set visibility on all of the related control points
            foreach (var o in DesignerPane.Children)
            {
                //Search for the control point that contains the element's ID
                //e.g Line1_StartPoint for Line1 element
                if (o is Ellipse)
                {
                    ((Ellipse) o).Visibility = ((Ellipse) o).Name.Contains(pathId)
                        ? Visibility.Visible
                        : Visibility.Hidden;
                }
            }*/
        }

        private void path_lockToSelected(UIElement element)
        {
            var pathId = ((Path)element).Name;
            //Search and set visibility on all of the related control points
            foreach(var o in DesignerPane.Children)
            {
                //Search for the control point that contains the element's ID
                //e.g Line1_StartPoint for Line1 element
                if (o is Ellipse)
                {
                    ((Ellipse)o).Visibility = ((Ellipse)o).Name.Contains(pathId)
                        ? Visibility.Visible
                        : Visibility.Hidden;
                }
            }
        }

        private void action_Done()
        {
            foreach (var o in DesignerPane.Children)
            {
                //Search for the control point that contains the element's ID
                //e.g Line1_StartPoint for Line1 element
                if (o is Ellipse)
                {
                    ((Ellipse)o).Visibility = Visibility.Hidden;
                }
            }
        }

        private void action_Delete(UIElement element)
        {
            var pathId = ((Path)element).Name;
            //Search and set visibility on all of the related control points
            for(int i = DesignerPane.Children.Count - 1; i >= 0; i--) {
                var o = DesignerPane.Children[i];
                if (o is Ellipse && ((Ellipse)o).Name.Contains(pathId))
                {
                    DesignerPane.Children.RemoveAt(i);
                }
            }
        }

        private void OnInsertGeometry(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = new Path
                    {
                        Stroke = Brushes.White,
                        StrokeThickness = 4
                    };
                var name = ((Button)sender).Content.ToString();
                var itemCount = _lineCount + _elliipseCount;
                GeometryBase gb;
                switch (name)
                {
                    case "Select":
                        if (itemCount == 0) { return; }
                        if (_selectedIndex != null) { ((Path)DrawingPane.Children[(int)_selectedIndex]).Stroke = Brushes.White; };
                        if(itemCount == 1 || _selectedIndex == null)
                        {
                            _selectedIndex = 0;

                        }
                        else
                        {
                            _selectedIndex = (_selectedIndex + 1) % itemCount;
                        }

                        ((Path)DrawingPane.Children[(int)_selectedIndex]).Stroke = Brushes.Green;

                        path_lockToSelected(DrawingPane.Children[(int)_selectedIndex]);

                        break;
                    case "Done":
                        if (_selectedIndex == null) { return; };

                        ((Path)DrawingPane.Children[(int)_selectedIndex]).Stroke = Brushes.White;

                        action_Done();
                        _selectedIndex = null;
                        break;
                    case "Delete":
                        if (itemCount == 0 || _selectedIndex == null) { return; }
                        
                        var element = (Path)DrawingPane.Children[(int)_selectedIndex];
                        if(element.Name.StartsWith("Ellipse")) { 
                            _elliipseCount--;
                        } else
                        { // TODO
                            _lineCount--;
                        }
                        action_Delete(element);

                        DrawingPane.Children.Remove(element);
                        _selectedIndex = null;

                        action_Done();
                        break;
                    case "Distance":
                        if (itemCount >= 3) { return; }
                        path.Name = "Line" + _lineCount;
                        gb = GeometryFactory("Line");
                        AddControlPoints(gb.ControlPoints, "Line");
                        path.Data = gb.CreateGeometry();

                        _currentElement = path;

                        _lineCount++;

                        if (_selectedIndex != null) { ((Path)DrawingPane.Children[(int)_selectedIndex]).Stroke = Brushes.White; };
                        _selectedIndex = DrawingPane.Children.Add(path);
                        path_lockToSelected(DrawingPane.Children[(int)_selectedIndex]);
                        ((Path)DrawingPane.Children[(int)_selectedIndex]).Stroke = Brushes.Green;

                        break;
                    case "Ellipse":
                        if (itemCount >= 3) { return; }

                        path.Name = "Ellipse" + _elliipseCount;

                        gb = GeometryFactory(name);
                        AddControlPoints(gb.ControlPoints, "Ellipse");

                        path.Data = gb.CreateGeometry();
                        _currentElement = path;

                        _elliipseCount++;
                        if (_selectedIndex != null ) { ((Path)DrawingPane.Children[(int)_selectedIndex]).Stroke = Brushes.White;  };
                        _selectedIndex = DrawingPane.Children.Add(path);
                        path_lockToSelected(DrawingPane.Children[(int)_selectedIndex]);
                        ((Path)DrawingPane.Children[(int)_selectedIndex]).Stroke = Brushes.Green;

                        break;
                    default:
                        throw new ApplicationException("Error:  Incorrect Geometry type");
                }
            }
            catch (ApplicationException argExcept)
            {
                MessageBox.Show(argExcept.Message);
            }
        }

        #endregion

        // ---------------------------------------------------------------------------------

        #region Drag and Move

        //Special variable for Drag and move actions
        private bool _isMoving;
        private Point _movingPreviousLocation;

        private static Point GetPerpendicularPoint(Point point1, Point point2, double RADIUS, bool isAbove)
        {
            double x1 = point1.X;
            double y1 = point1.Y;
            double x2 = point2.X;
            double y2 = point2.Y;
            // Calculate the slope of the line passing through points 1 and 2
            double dx = x2 - x1;
            double dy = y2 - y1;

            // Check if the line is vertical
            if (dx == 0)
            {
                // The line is vertical, so the perpendicular line is horizontal
                // Move RADIUS units to the right (above) or left (below) of point 2
                double new_x = x2 + (isAbove ? RADIUS : -RADIUS);
                double new_y = y2;
                return new Point(new_x, new_y);
            }
            else
            {
                // Calculate the slope of the line perpendicular to the line passing through points 1 and 2
                double slope_perpendicular = -dx / dy;

                // Calculate the displacement in the x and y directions
                double displacement_dx = RADIUS / Math.Sqrt(1 + slope_perpendicular * slope_perpendicular);
                double displacement_dy = slope_perpendicular * displacement_dx;

                // Adjust the sign of the displacement based on the isAbove flag
                if (!isAbove)
                {
                    displacement_dx = -displacement_dx;
                    displacement_dy = -displacement_dy;
                }

                // Add the displacement vector to the coordinates of point 2 to get the coordinates of the new point
                double new_x = x2 + displacement_dx;
                double new_y = y2 + displacement_dy;
                return new Point(new_x, new_y);
            }
        }

        private Point getJumpPoint(Point center, Point point2, double distance)
        {
            // Define the coordinates of points 1 and 2
            double x1 = center.X;
            double y1 = center.Y;
            double x2 = point2.X;
            double y2 = point2.Y;

            // Calculate the slope of the line passing through points 1 and 2
            double dx = x2 - x1;
            double dy = y2 - y1;

            if(dx + dy == 0)
            {
                return center;
            }

            // Check if the line is vertical
            if (dx == 0)
            {
                // The line is vertical, so we move up or down along the y-axis
                double new_x = x2;
                double new_y = y2 + Math.Sign(dy) * distance;
                return new Point(new_x, new_y);
            }
            else
            {
                // Calculate the angle of the line with respect to the x-axis
                double angle = Math.Atan2(dy, dx);

                // Calculate the displacement in the x and y directions
                double displacement_dx = distance * Math.Cos(angle);
                double displacement_dy = distance * Math.Sin(angle);

                // Add the displacement vector to the coordinates of point 2 to get the coordinates of the new point
                double new_x = x2 + displacement_dx;
                double new_y = y2 + displacement_dy;
                return new Point(new_x, new_y);
        }
    }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var el = (Ellipse) sender;
            var eg = new Point(Canvas.GetLeft(el), Canvas.GetTop(el));
            el.Cursor = Cursors.Hand;
            var s = el.Name.Split('_');
            // jump for visibility
            if (!_isMoving)
            {
                Point movingEndLocation;
                movingEndLocation = e.GetPosition(DrawingPane);
                var eCenter =
                            LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_Center") as Ellipse;

                var egC = new Point(Canvas.GetLeft(eCenter), Canvas.GetTop(eCenter));
                var jumpPoint = getJumpPoint(egC, eg, 50);

                /* Canvas.SetLeft(el, movingEndLocation.X + 10); // todo: jump in the direction away from center
                 Canvas.SetTop(el, movingEndLocation.Y + 10);*/
                Canvas.SetLeft(el, jumpPoint.X); // todo: jump in the direction away from center
                Canvas.SetTop(el, jumpPoint.Y);

                //dateGeometries(jumpPoint, el.Name);
            }
            _isMoving = true;

            // _movingPreviousLocation = e.GetPosition(DrawingPane);
        }

        private void Ellipse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var el = (Ellipse) sender;
            el.Cursor = Cursors.Arrow;
            _isMoving = false;
        }


        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            var el = (Ellipse) sender;
            Point movingEndLocation;
            if (_isMoving)
            {
                movingEndLocation = e.GetPosition(DrawingPane);

                Canvas.SetLeft(el, movingEndLocation.X - el.Width/2);
                Canvas.SetTop(el, movingEndLocation.Y - el.Height/2);

                UpdateGeometries(movingEndLocation, el.Name);
                _movingPreviousLocation = movingEndLocation;
            }
        }

        #endregion

        // ---------------------------------------------------------------------------------

        #region private helper functions

        private void UpdateControlPoints(ArrayList controlPoints)
        {
            var geometryType = GetGeometryTypeInId(_currentElement.Name);

            switch (geometryType)
            {
                case "Line":
                    UpdateLineGeometryControlPoints(controlPoints);
                    break;
                case "Ellipse":
                    UpdateEllipseGeometryControlPoints(controlPoints);
                    break;
                default:
                    throw new ApplicationException("Error: incorrect Element name");
            }
        }


        private void UpdateLineGeometryControlPoints(ArrayList controlPoints)
        {
            if (controlPoints.Count != 2)
                throw new ApplicationException("Error:  incorrect # of control points for LineGeometry");

            for (var i = 0; i < controlPoints.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        var eStartPoint =
                            LogicalTreeHelper.FindLogicalNode(DesignerPane, _currentElement.Name + "_StartPoint") as
                                Ellipse;
                        Canvas.SetLeft(eStartPoint, ((Point) controlPoints[i]).X - eStartPoint.Width/2);
                        Canvas.SetTop(eStartPoint, ((Point) controlPoints[i]).Y - eStartPoint.Height/2);
                        break;
                    case 1:
                        var eEndPoint =
                            LogicalTreeHelper.FindLogicalNode(DesignerPane, _currentElement.Name + "_EndPoint") as
                                Ellipse;
                        Canvas.SetLeft(eEndPoint, ((Point) controlPoints[i]).X - eEndPoint.Width/2);
                        Canvas.SetTop(eEndPoint, ((Point) controlPoints[i]).Y - eEndPoint.Height/2);
                        break;
                    default:
                        throw new ApplicationException("Error: incorrect # of control points in LineG");
                }
            }
        }

        private void UpdateEllipseGeometryControlPoints(ArrayList controlPoints)
        {
            if (controlPoints.Count != 5)
            {
                throw new ApplicationException("Error:  incorrect # of control points for EllipseGeometry");
            }
            for (var i = 0; i < controlPoints.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        var eCenter =
                            LogicalTreeHelper.FindLogicalNode(DesignerPane, _currentElement.Name + "_Center") as Ellipse;
                        Canvas.SetLeft(eCenter, ((Point) controlPoints[i]).X - eCenter.Width/2);
                        Canvas.SetTop(eCenter, ((Point) controlPoints[i]).Y - eCenter.Height/2);
                        break;
                    case 1:
                        var eTopLeft =
                            LogicalTreeHelper.FindLogicalNode(DesignerPane, _currentElement.Name + "_TopLeft") as
                                Ellipse;
                        Canvas.SetLeft(eTopLeft, ((Point) controlPoints[i]).X - eTopLeft.Width/2);
                        Canvas.SetTop(eTopLeft, ((Point) controlPoints[i]).Y - eTopLeft.Height/2);
                        break;
                    case 2:
                        var eTopMiddle =
                            LogicalTreeHelper.FindLogicalNode(DesignerPane, _currentElement.Name + "_TopMiddle") as
                                Ellipse;
                        Canvas.SetLeft(eTopMiddle, ((Point) controlPoints[i]).X - eTopMiddle.Width/2);
                        Canvas.SetTop(eTopMiddle, ((Point) controlPoints[i]).Y - eTopMiddle.Height/2);
                        break;
                    case 3:
                        var eTopRight =
                            LogicalTreeHelper.FindLogicalNode(DesignerPane, _currentElement.Name + "_TopRight") as
                                Ellipse;
                        Canvas.SetLeft(eTopRight, ((Point) controlPoints[i]).X - eTopRight.Width/2);
                        Canvas.SetTop(eTopRight, ((Point) controlPoints[i]).Y - eTopRight.Height/2);
                        break;
                    case 4:
                        var eBottomMiddle =
                            LogicalTreeHelper.FindLogicalNode(DesignerPane, _currentElement.Name + "_BottomMiddle") as
                                Ellipse;
                        Canvas.SetLeft(eBottomMiddle, ((Point) controlPoints[i]).X - eBottomMiddle.Width/2);
                        Canvas.SetTop(eBottomMiddle, ((Point) controlPoints[i]).Y - eBottomMiddle.Height/2);
                        break;
                    default:
                        throw new ApplicationException("Error: incorrect # of control points in EllipseG");
                }
            }
        }

        private void UpdateGeometries(Point movingEndLocation, string id)
        {
            var geometryType = GetGeometryTypeInId(id);

            //Switch the controlpoint pane to the right panel
            // GeometryPaneChange(geometryType, false);

            switch (geometryType)
            {
                case "Line":
                    UpdateLineGeometry(movingEndLocation, id);
                    break;
                case "Ellipse":
                    UpdateEllipseGeometry(movingEndLocation, id);
                    break;
                default:
                    throw new ApplicationException("Error: incorrect GeometryType in UpdateGeometries()");
            }
        }

        private object SearchUpdatedElement(string p) => DrawingPane.Children.Cast<object>().FirstOrDefault(o => o is Path && ((Path)o).Name == p);

        private void UpdateLineGeometry(Point movingEndLocation, string id)
        {
            var s = id.Split('_');
            var controlPointType = GetContronPointTypeInId(id);
            var p = SearchUpdatedElement(s[0]) as Path;
            if (p == null)
            {
                throw new ApplicationException("Error: incorrect geometry ID");
            }
            _currentElement = p;
            var lg = p.Data as LineGeometry;

            switch (controlPointType)
            {
                case "StartPoint":
                    lg.StartPoint = movingEndLocation;

                    break;
                case "EndPoint":
                    lg.EndPoint = movingEndLocation;

                    break;
                default:
                    throw new ApplicationException("Error: Incorrect controlpoint type, '" + controlPointType +
                                                   "' in UpdateLineGeometry()");
            }
            var xamlstring = XamlWriter.Save(DrawingPane);
            ((TextBox) XAMLPane.Children[0]).Text = xamlstring;
        }

        private void UpdateEllipseGeometry(Point movingEndLocation, string id)
        {
            var s = id.Split('_');
            var controlPointType = GetContronPointTypeInId(id);
            var p = SearchUpdatedElement(s[0]) as Path;
            if (p == null)
            {
                throw new ApplicationException("Error: incorrect geometry ID");
            }
            _currentElement = p;
            var eg = p.Data.Clone() as EllipseGeometry;
            var diffX = movingEndLocation.X - eg.Center.X;
            var diffY = movingEndLocation.Y - eg.Center.Y;

            double radians, angle;
            radians = Math.Atan2(diffY, diffX);
            angle = radians*(180/Math.PI);

            

            switch (controlPointType)
            {
                case "Center":
                    eg.Center = movingEndLocation;

                    //Update the center in the RotateTransform when the ellipsegeometry's center is moved
                    if (eg.Transform is RotateTransform)
                    {
                        var rtWithNewCenter = eg.Transform as RotateTransform;
                        rtWithNewCenter.CenterX = eg.Center.X;
                        rtWithNewCenter.CenterY = eg.Center.Y;
                    }
                    foreach (var o in DesignerPane.Children)
                    {
                        if (o is Ellipse && ((Ellipse) o).Name.Contains(s[0]) && ((Ellipse) o).Name != s[0])
                        {
                            Canvas.SetLeft((Ellipse)o, Canvas.GetLeft(((Ellipse)o)) + diffX);
                            Canvas.SetTop(((Ellipse) o), Canvas.GetTop(((Ellipse) o)) + diffY);
                        }
                    }
                    p.Data = eg;

                    break;

                case "TopLeft":
                    var v0 = new Vector(-eg.RadiusX, 0);
                    var v1 = new Vector(diffX, diffY);

                    var rtTopLeft = new RotateTransform(angle + 180, eg.Center.X, eg.Center.Y);
                    eg.Transform = rtTopLeft;
                    eg.RadiusX = v1.Length;
                    p.Data = eg;

                    var eTopRight = (Ellipse)LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_TopRight");

                    var rightPoint = new Point(Canvas.GetLeft(eTopRight), Canvas.GetTop(eTopRight));
                    var topPoint = new Point(Canvas.GetLeft(eTopRight), Canvas.GetTop(eTopRight));
                    var bottomPoint = new Point(Canvas.GetLeft(eTopRight), Canvas.GetTop(eTopRight));
                    var radius = Math.Sqrt(Math.Pow((topPoint.X - bottomPoint.X), 2) + Math.Pow((topPoint.Y - bottomPoint.Y),2)) / 2;


                    var eCenter =
                        (Ellipse)LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_Center");
                    
                    var eTopMiddle = (Ellipse) LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_TopMiddle");

                    var eBottomMiddle =
                        (Ellipse)LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_BottomMiddle");

                    var newCenter = new Point((rightPoint.X + movingEndLocation.X) / 2, (rightPoint.Y + movingEndLocation.Y) / 2);
                    //set center
                    Canvas.SetLeft(eCenter, newCenter.X);
                    Canvas.SetTop(eCenter, newCenter.Y);

                    var newTop = GetPerpendicularPoint(movingEndLocation, newCenter, eg.RadiusY, true);
                    var newBottom = GetPerpendicularPoint(movingEndLocation, newCenter, eg.RadiusY, false);
                    //set top
                    Canvas.SetLeft(eTopMiddle, newTop.X);
                    Canvas.SetTop(eTopMiddle, newTop.Y);

                    //set bottom
                    Canvas.SetLeft(eBottomMiddle, newBottom.X);
                    Canvas.SetTop(eBottomMiddle, newBottom.Y);

                    break;
                case "TopMiddle":
                    var v1TopMiddle = new Vector(diffX, diffY);
                    var rtTopMiddle = new RotateTransform(angle + 90, eg.Center.X, eg.Center.Y);
                    eg.Transform = rtTopMiddle;
                    eg.RadiusY = v1TopMiddle.Length;
                    p.Data = eg;

                    var eTopLeft2 = (Ellipse) LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_TopLeft");
                    Canvas.SetLeft(eTopLeft2, (eg.Center.X + diffY*eg.RadiusX/v1TopMiddle.Length) - eTopLeft2.Width/2);
                    Canvas.SetTop(eTopLeft2, (eg.Center.Y - diffX*eg.RadiusX/v1TopMiddle.Length) - eTopLeft2.Height/2);

                    var eTopRight2 = (Ellipse) LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_TopRight");
                    Canvas.SetLeft(eTopRight2, (eg.Center.X - diffY*eg.RadiusX/v1TopMiddle.Length) - eTopRight2.Width/2);
                    Canvas.SetTop(eTopRight2, (eg.Center.Y + diffX*eg.RadiusX/v1TopMiddle.Length) - eTopRight2.Height/2);

                    var eBottomMiddle2 =
                        (Ellipse) LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_BottomMiddle");
                    Canvas.SetLeft(eBottomMiddle2, (eg.Center.X - diffX) - eBottomMiddle2.Width/2);
                    Canvas.SetTop(eBottomMiddle2, (eg.Center.Y - diffY) - eBottomMiddle2.Height/2);
                    break;

                case "TopRight":
                    var v1TopRight = new Vector(diffX, diffY);
                    var rtTopRight = new RotateTransform(angle, eg.Center.X, eg.Center.Y);
                    eg.Transform = rtTopRight;
                    eg.RadiusX = v1TopRight.Length;
                    p.Data = eg;

                    var eTopLeft3 = (Ellipse) LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_TopLeft");
                    Canvas.SetLeft(eTopLeft3, (eg.Center.X - diffX) - eTopLeft3.Width/2);
                    Canvas.SetTop(eTopLeft3, (eg.Center.Y - diffY) - eTopLeft3.Height/2);

                    var eTopMiddle3 = (Ellipse) LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_TopMiddle");
                    Canvas.SetLeft(eTopMiddle3, (eg.Center.X + diffY*eg.RadiusY/v1TopRight.Length) - eTopMiddle3.Width/2);
                    Canvas.SetTop(eTopMiddle3, (eg.Center.Y - diffX*eg.RadiusY/v1TopRight.Length) - eTopMiddle3.Height/2);

                    var eBottomMiddle3 =
                        (Ellipse) LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_BottomMiddle");
                    Canvas.SetLeft(eBottomMiddle3,
                        (eg.Center.X - diffY*eg.RadiusY/v1TopRight.Length) - eBottomMiddle3.Width/2);
                    Canvas.SetTop(eBottomMiddle3,
                        (eg.Center.Y + diffX*eg.RadiusY/v1TopRight.Length) - eBottomMiddle3.Height/2);
                    break;

                case "BottomMiddle":
                    var v1BottomMiddle = new Vector(diffX, diffY);
                    var rtBottomMiddle = new RotateTransform(angle - 90, eg.Center.X, eg.Center.Y);
                    eg.Transform = rtBottomMiddle;
                    eg.RadiusY = v1BottomMiddle.Length;
                    p.Data = eg;

                    var eTopLeft4 = (Ellipse) LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_TopLeft");
                    Canvas.SetLeft(eTopLeft4, (eg.Center.X - diffY*eg.RadiusX/v1BottomMiddle.Length) - eTopLeft4.Width/2);
                    Canvas.SetTop(eTopLeft4, (eg.Center.Y + diffX*eg.RadiusX/v1BottomMiddle.Length) - eTopLeft4.Height/2);

                    var eTopRight4 = (Ellipse) LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_TopRight");
                    Canvas.SetLeft(eTopRight4,
                        (eg.Center.X + diffY*eg.RadiusX/v1BottomMiddle.Length) - eTopRight4.Width/2);
                    Canvas.SetTop(eTopRight4,
                        (eg.Center.Y - diffX*eg.RadiusX/v1BottomMiddle.Length) - eTopRight4.Height/2);

                    var eTopMiddle4 = (Ellipse) LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_TopMiddle");
                    Canvas.SetLeft(eTopMiddle4, (eg.Center.X - diffX) - eTopMiddle4.Width/2);
                    Canvas.SetTop(eTopMiddle4, (eg.Center.Y - diffY) - eTopMiddle4.Height/2);
                    break;

                default:
                    throw new ApplicationException("Error: incorrect EllipseGeometry controlpoint type");
            }
            var xamlstring = XamlWriter.Save(DrawingPane);
            ((TextBox) XAMLPane.Children[0]).Text = xamlstring;
        }

        private string GetContronPointTypeInId(string id)
        {
            var s = id.Split('_');
            return s[1];
        }

        private string GetGeometryTypeInId(string id)
        {
            var s = id.Split('_');
            if (s[0].Contains("Line"))
            {
                return "Line";
            }
            if (s[0].Contains("Ellipse"))
            {
                return "Ellipse";
            }
            if (s[0].Contains("Rectangle"))
            {
                return "Rectangle";
            }
            if (s[0].Contains("Arc"))
            {
                return "Arc";
            }
            return "Bezier";
        }

        private void AddControlPoints(ArrayList controlPoints, string geometryType)
        {
            switch (geometryType)
            {
                case "Line":
                    AddLineGeometryControlPoints(controlPoints);
                    break;
                case "Ellipse":
                    AddEllipseGeometryControlPoints(controlPoints);
                    break;
                default:
                    throw new ApplicationException("Error:  incorrect Geometry type");
            }
        }


        private static readonly double ControlPointMarkerWidth = 20;
        private static readonly double ControlPointMarkerHeight = 20;

        private void deleteElement(UIElement element)
        {
            /*
             {
                                e.Name = "Line" + _lineCount + "_StartPoint";
                            }
                            else
                            {
                                e.Name = "Line" + _lineCount + "_EndPoint";
                            }*/


        }

        private void AddLineGeometryControlPoints(ArrayList controlPoints)
        {
            if (controlPoints.Count != 2)
                throw new ApplicationException("Error:  incorrect # of control points for LineGeometry");

            for (var i = 0; i < controlPoints.Count; i++)
            {
                var e = new Ellipse
                {
                    Visibility = Visibility.Hidden,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Fill = Brushes.White,
                    Opacity = 0.5,
                    Width = 3,
                    Height = 3
                };

                if (i == 0)
                {
                    e.Name = "Line" + _lineCount + "_StartPoint";
                }
                else
                {
                    e.Name = "Line" + _lineCount + "_EndPoint";
                }

                e.Width = ControlPointMarkerWidth;
                e.Height = ControlPointMarkerHeight;
                Canvas.SetLeft(e, ((Point) controlPoints[i]).X - e.Width/2);
                Canvas.SetTop(e, ((Point) controlPoints[i]).Y - e.Height/2);


                e.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;
                e.MouseLeftButtonUp += Ellipse_MouseLeftButtonUp;
                e.MouseMove += Ellipse_MouseMove;

                //Add the control point to the Designer Pane
                //DesignerPane.Children.Add(e);
                DesignerPane.Children.Insert(DesignerPane.Children.Count - 1, e);
            }
        }

        private void AddEllipseGeometryControlPoints(ArrayList controlPoints)
        {
            if (controlPoints.Count != 5)
            {
                throw new ApplicationException("Error:  incorrect # of control points for EllipseGeometry");
            }
            for (var i = 0; i < controlPoints.Count; i++)
            {
                var e = new Ellipse
                {
                    Visibility = Visibility.Hidden,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Fill = Brushes.White,
                    Opacity = 0.5,
                    Width = 3,
                    Height = 3
                };

                switch (i)
                {
                    case 0:
                        e.Name = "Ellipse" + _elliipseCount + "_Center";
                        break;
                    case 1:
                        e.Name = "Ellipse" + _elliipseCount + "_TopLeft";
                        break;
                    case 2:
                        e.Name = "Ellipse" + _elliipseCount + "_TopMiddle";
                        break;
                    case 3:
                        e.Name = "Ellipse" + _elliipseCount + "_TopRight";
                        break;
                    case 4:
                        e.Name = "Ellipse" + _elliipseCount + "_BottomMiddle";
                        break;
                    default:
                        throw new ApplicationException("Error: Incorrect control point ");
                }

                e.Width = ControlPointMarkerWidth;
                e.Height = ControlPointMarkerHeight;
                Canvas.SetLeft(e, ((Point) controlPoints[i]).X - e.Width/2);
                Canvas.SetTop(e, ((Point) controlPoints[i]).Y - e.Height/2);

                e.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;
                e.MouseLeftButtonUp += Ellipse_MouseLeftButtonUp;
                e.MouseMove += Ellipse_MouseMove;

                //Add the control point to the Designer Pane
                DesignerPane.Children.Insert(DesignerPane.Children.Count - 1, e);
            }
        }

        /// <summary>
        ///     The function takes the design pane element,e.g. LinePane, RectanglePane,
        ///     and extract the value from the different control point fields from the pane and construct
        ///     a Path with the correct Geometry
        /// </summary>
        /// <param name="pane"></param>
        /// <returns></returns>
        private GeometryBase GeometryFactory(String name)
        {
            switch (name)
            {
                case "Line":
                    return new LineG();
                case "Ellipse":
                    return new EllipseG();
                default:
                    throw new ApplicationException("Error:  Unknow Geometry name?");
            }
        }

        #endregion

        #region Data members

        private static int _lineCount = 0;
        private static int _elliipseCount = 0;
        private string _selectedMeasure;
        private static int? _selectedIndex;
        private bool _isShow;
        private Path _currentElement;

        #endregion
    }
}