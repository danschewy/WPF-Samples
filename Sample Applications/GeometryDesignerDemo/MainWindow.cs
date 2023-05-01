// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                _centerPoint = new Point(((_startPoint.X + _endPoint.X) / 2), (_startPoint.Y + _endPoint.Y) / 2);
                ControlPoints.Add(_startPoint);
                ControlPoints.Add(_endPoint);
                ControlPoints.Add(_centerPoint);
            }

            public override Geometry CreateGeometry()
            {
                var lg = new LineGeometry(_startPoint, _endPoint);
                return lg;
            }

            #region Data members

            private Point _startPoint;
            private Point _endPoint;
            private Point _centerPoint;

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
        public enum Direction
        {
            Left,
            Right,
            Top,
            Bottom
        }
        private static Point GetPerpendicularPoint(Point point1, Point point2, double RADIUS, Direction direction)
        {
            double x1 = point1.X;
            double y1 = point1.Y;
            double x2 = point2.X;
            double y2 = point2.Y;

            // Calculate the slope of the line passing through points 1 and 2
            double dx = x2 - x1;
            double dy = y2 - y1;

            // Calculate the displacement in the x and y directions based on the specified direction
            double displacement_dx = 0;
            double displacement_dy = 0;

            var isTop = direction == Direction.Top | direction == Direction.Left;
            var isBottom = direction == Direction.Bottom | direction == Direction.Right;

            if (dx == 0) // Vertical line
            {
               displacement_dx = isTop ? RADIUS : -RADIUS;
                displacement_dy = 0;
            }
            else if (dy == 0) // Horizontal line
            {
                displacement_dx = 0;
                displacement_dy= (isTop? RADIUS : -RADIUS);
            }
            else // Non-horizontal and non-vertical line
            {
                double slope_perpendicular = -dx / dy;
                double magnitude = Math.Sqrt(1 + slope_perpendicular * slope_perpendicular);
                double unit_dx = 1 / magnitude;
                double unit_dy = slope_perpendicular * unit_dx;

                if (direction == Direction.Left)
                {
                    displacement_dx = -RADIUS * unit_dy;
                    displacement_dy = RADIUS * unit_dx;
                }
                else if (direction == Direction.Right)
                {
                    displacement_dx = RADIUS * unit_dy;
                    displacement_dy = -RADIUS * unit_dx;
                }
                else if (direction == Direction.Top)
                {
                    displacement_dx = RADIUS * unit_dx;
                    displacement_dy = RADIUS * unit_dy;
                }
                else // Direction.Bottom
                {
                    displacement_dx = -RADIUS * unit_dx;
                    displacement_dy = -RADIUS * unit_dy;
                }
            }

            // Add the displacement vector to the coordinates of point 2 to get the coordinates of the new point
            double new_x = x2 + displacement_dx;
            double new_y = y2 + displacement_dy;
            return new Point(new_x, new_y);
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
            if (controlPoints.Count != 3)
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
                    case 2:
                        var eCenterPoint =
                            LogicalTreeHelper.FindLogicalNode(DesignerPane, _currentElement.Name + "_Center") as
                                Ellipse;
                        Canvas.SetLeft(eCenterPoint, ((Point)controlPoints[i]).X - eCenterPoint.Width / 2);
                        Canvas.SetTop(eCenterPoint, ((Point)controlPoints[i]).Y - eCenterPoint.Height / 2);
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
            var center = new Point((lg.StartPoint.X + lg.EndPoint.X) / 2, (lg.StartPoint.Y + lg.EndPoint.Y) / 2);
            var eCenter =
                (Ellipse)LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_Center");
            var w = eCenter.Width / 2;
            var h = eCenter.Height / 2;
            switch (controlPointType)
            {
                case "StartPoint":
                    lg.StartPoint = movingEndLocation;
                    Canvas.SetLeft(eCenter, center.X - w);
                    Canvas.SetTop(eCenter, center.Y - h);
                    break;
                case "EndPoint":
                    lg.EndPoint = movingEndLocation;
                    Canvas.SetLeft(eCenter, center.X - w);
                    Canvas.SetTop(eCenter, center.Y - h);
                    break;
                case "Center":
                    var diffX = movingEndLocation.X - center.X;
                    var diffY = movingEndLocation.Y - center.Y;
                    lg.StartPoint = new Point(lg.StartPoint.X + diffX, lg.StartPoint.Y + diffY);
                    lg.EndPoint = new Point(lg.EndPoint.X + diffX, lg.EndPoint.Y + diffY);
                    foreach (var o in DesignerPane.Children)
                    {
                        if (o is Ellipse && ((Ellipse)o).Name.Contains(s[0]) && ((Ellipse)o).Name != s[0])
                        {
                            Canvas.SetLeft((Ellipse)o, Canvas.GetLeft(((Ellipse)o)) + diffX);
                            Canvas.SetTop(((Ellipse)o), Canvas.GetTop(((Ellipse)o)) + diffY);
                        }
                    }

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

            var eCenter =
                (Ellipse)LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_Center");
            var eTopMiddle = (Ellipse)LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_TopMiddle");
            var eBottomMiddle =
                (Ellipse)LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_BottomMiddle");
            var eTopLeft = (Ellipse)LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_TopLeft");
            var eTopRight = (Ellipse)LogicalTreeHelper.FindLogicalNode(DesignerPane, s[0] + "_TopRight");
            var w = eCenter.Width / 2;
            var h = eCenter.Height / 2;
            var leftPoint = new Point(Canvas.GetLeft(eTopLeft) + w, Canvas.GetTop(eTopLeft) + h);
            var rightPoint = new Point(Canvas.GetLeft(eTopRight) + w, Canvas.GetTop(eTopRight) + h);
            var topPoint = new Point(Canvas.GetLeft(eTopMiddle) + w, Canvas.GetTop(eTopMiddle) + h);
            var bottomPoint = new Point(Canvas.GetLeft(eBottomMiddle) + w, Canvas.GetTop(eBottomMiddle) + h);
            
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
                    var newCenter = new Point(((rightPoint.X + movingEndLocation.X) / 2), (rightPoint.Y + movingEndLocation.Y) / 2);
                    
                    var newTop = GetPerpendicularPoint(movingEndLocation, newCenter, eg.RadiusY, Direction.Top);
                    var newBottom = GetPerpendicularPoint(movingEndLocation, newCenter, eg.RadiusY, Direction.Bottom);

                    var radiusX = Math.Sqrt(Math.Pow((movingEndLocation.X - rightPoint.X), 2) + Math.Pow((movingEndLocation.Y - rightPoint.Y),2)) / 2;
                    var radiusY = Math.Sqrt(Math.Pow((newTop.X - newBottom.X), 2) + Math.Pow((newTop.Y - newBottom.Y),2)) / 2;

                    // Calculate the differences in the x and y coordinates
                    double dx = movingEndLocation.X - rightPoint.X;
                    double dy = movingEndLocation.Y - rightPoint.Y;

                    // Calculate the angle in radians
                    double angleRadians = Math.Atan2(dy, dx);

                    // Convert the angle from radians to degrees
                    double angleDegrees = angleRadians * (180.0 / Math.PI);

                    var rtTopLeft = new RotateTransform(angleDegrees, newCenter.X, newCenter.Y);

                    eg.Transform = rtTopLeft;
                    eg.Center = newCenter;
                    eg.RadiusX = radiusX;
                    p.Data = eg;

                    //set center
                    Canvas.SetLeft(eCenter, newCenter.X - w);
                    Canvas.SetTop(eCenter, newCenter.Y - h);
                    
                    //set top
                    Canvas.SetLeft(eTopMiddle, newTop.X - w);
                    Canvas.SetTop(eTopMiddle, newTop.Y - h);

                    //set bottom
                    Canvas.SetLeft(eBottomMiddle, newBottom.X - w);
                    Canvas.SetTop(eBottomMiddle, newBottom.Y - h);

                    break;
                case "TopMiddle":
                    var newCenter3 = new Point(((bottomPoint.X + movingEndLocation.X) / 2), (bottomPoint.Y + movingEndLocation.Y) / 2);

                    var newLeft3 = GetPerpendicularPoint(movingEndLocation, newCenter3, eg.RadiusX, Direction.Top);
                    var newRight3 = GetPerpendicularPoint(movingEndLocation, newCenter3, eg.RadiusX, Direction.Bottom);

                    var radiusY3 = Math.Sqrt(Math.Pow((bottomPoint.X - movingEndLocation.X), 2) + Math.Pow((bottomPoint.Y - movingEndLocation.Y), 2)) / 2;
                    var radiusX3 = Math.Sqrt(Math.Pow((newLeft3.X - newRight3.X), 2) + Math.Pow((newLeft3.Y - newRight3.Y), 2)) / 2;
                    // Calculate the differences in the x and y coordinates
                    double dx3 = bottomPoint.X - movingEndLocation.X;
                    double dy3 = bottomPoint.Y - movingEndLocation.Y;

                    // Calculate the angle in radians
                    double angleRadians3= Math.Atan2(dy3, dx3);

                    // Convert the angle from radians to degrees
                    double angleDegrees3 = angleRadians3* (180.00 / Math.PI) - 90;

                    var rtTopMiddle = new RotateTransform(angleDegrees3, newCenter3.X, newCenter3.Y);

                    eg.Transform = rtTopMiddle;
                    eg.Center = newCenter3;
                    eg.RadiusY = radiusY3;
                    p.Data = eg;

                    //set center
                    Canvas.SetLeft(eCenter, newCenter3.X - w);
                    Canvas.SetTop(eCenter, newCenter3.Y - h);

                    //set left
                    Canvas.SetLeft(eTopLeft, newLeft3.X - w);
                    Canvas.SetTop(eTopLeft, newLeft3.Y - h);

                    //set right
                    Canvas.SetLeft(eTopRight, newRight3.X - w);
                    Canvas.SetTop(eTopRight, newRight3.Y - h);

                    break;

                case "TopRight":
                    var newCenter1 = new Point(((leftPoint.X + movingEndLocation.X) / 2), (leftPoint.Y + movingEndLocation.Y) / 2);

                    var newTop1 = GetPerpendicularPoint(movingEndLocation, newCenter1, eg.RadiusY, Direction.Top);
                    var newBottom1 = GetPerpendicularPoint(movingEndLocation, newCenter1, eg.RadiusY, Direction.Bottom);

                    var radiusX1 = Math.Sqrt(Math.Pow((movingEndLocation.X - leftPoint.X), 2) + Math.Pow((movingEndLocation.Y - leftPoint.Y), 2)) / 2;
                    var radiusY1 = Math.Sqrt(Math.Pow((newTop1.X - newBottom1.X), 2) + Math.Pow((newTop1.Y - newBottom1.Y), 2)) / 2;

                    // Calculate the differences in the x and y coordinates
                    double dx1 = movingEndLocation.X - leftPoint.X;
                    double dy1 = movingEndLocation.Y - leftPoint.Y;

                    // Calculate the angle in radians
                    double angleRadians1 = Math.Atan2(dy1, dx1);

                    // Convert the angle from radians to degrees
                    double angleDegrees1 = angleRadians1 * (180.0 / Math.PI);

                    var transforms1 = new TransformGroup();
                    var rtTopRight = new RotateTransform(angleDegrees1, newCenter1.X, newCenter1.Y);

                    eg.Transform = rtTopRight;
                    eg.Center = newCenter1;
                    eg.RadiusX = radiusX1;
                    p.Data = eg;

                    //set center
                    Canvas.SetLeft(eCenter, newCenter1.X - w);
                    Canvas.SetTop(eCenter, newCenter1.Y - h);

                    //set top
                    Canvas.SetLeft(eTopMiddle, newTop1.X - w);
                    Canvas.SetTop(eTopMiddle, newTop1.Y - h);

                    //set bottom
                    Canvas.SetLeft(eBottomMiddle, newBottom1.X - w);
                    Canvas.SetTop(eBottomMiddle, newBottom1.Y - h);

                    break;
                case "BottomMiddle":
                    var newCenter2 = new Point(((topPoint.X + movingEndLocation.X) / 2), (topPoint.Y + movingEndLocation.Y) / 2);

                    var newLeft= GetPerpendicularPoint(movingEndLocation, newCenter2, eg.RadiusX, Direction.Top);
                    var newRight = GetPerpendicularPoint(movingEndLocation, newCenter2, eg.RadiusX, Direction.Bottom);

                    var radiusY2 = Math.Sqrt(Math.Pow((topPoint.X - movingEndLocation.X), 2) + Math.Pow((topPoint.Y - movingEndLocation.Y), 2)) / 2;
                    var radiusX2 = Math.Sqrt(Math.Pow((newLeft.X - newRight.X), 2) + Math.Pow((newLeft.Y - newRight.Y), 2)) / 2;
                    // Calculate the differences in the x and y coordinates
                    double dx2 = topPoint.X - movingEndLocation.X;
                    double dy2 = topPoint.Y - movingEndLocation.Y;

                    // Calculate the angle in radians
                    double angleRadians2 = Math.Atan2(dy2, dx2);

                    // Convert the angle from radians to degrees
                    double angleDegrees2 = angleRadians2 * (180.00 / Math.PI) + 90;

                    var transforms2 = new TransformGroup();
                    var rtBottomMiddle = new RotateTransform(angleDegrees2, newCenter2.X, newCenter2.Y);

                    eg.Transform = rtBottomMiddle;
                    eg.Center = newCenter2;
                    eg.RadiusY= radiusY2;
                    p.Data = eg;

                    //set center
                    Canvas.SetLeft(eCenter, newCenter2.X - w);
                    Canvas.SetTop(eCenter, newCenter2.Y - h);

                    //set left
                    Canvas.SetLeft(eTopLeft, newLeft.X - w);
                    Canvas.SetTop(eTopLeft, newLeft.Y - h);

                    //set right
                    Canvas.SetLeft(eTopRight, newRight.X - w);
                    Canvas.SetTop(eTopRight, newRight.Y - h);

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
            if (controlPoints.Count != 3)
                throw new ApplicationException("Error:  incorrect # of control points for LineGeometry");

            for (var i = 0; i < controlPoints.Count; i++)
            {
                var e = new Ellipse
                {
                    Visibility = Visibility.Hidden,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Fill = _lineCount == 0 ? Brushes.White : _lineCount == 1 ? Brushes.LightCoral : Brushes.LightGreen,
                    Opacity = 0.5,
                    Width = 3,
                    Height = 3
                };

                if (i == 0)
                {
                    e.Name = "Line" + _lineCount + "_StartPoint";
                }
                else if(i == 1)
                {
                    e.Name = "Line" + _lineCount + "_EndPoint";
                }
                else
                {
                    e.Name = "Line" + _lineCount + "_Center";
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
                    Fill = _elliipseCount == 0 ? Brushes.White : _elliipseCount == 1 ? Brushes.LightCoral : Brushes.LightGreen,
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