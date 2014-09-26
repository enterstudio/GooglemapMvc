﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.UI;
using Jmelosegui.Mvc.Googlemap.Overlays;

namespace Jmelosegui.Mvc.Googlemap
{
    public class GoogleMap
    {
        #region Public Properties

        public string Id { get; internal set; }

        public string ApiKey { get; internal set; }

        public GoogleMapClientEvents ClientEvents { get; private set; }

        public CultureInfo Culture { get; set; }

        public bool DisableDoubleClickZoom { get; set; }

        public bool Draggable { get; set; }

        public bool EnableMarkersClustering { get; set; }

        public int Height { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public MapType MapType { get; set; }

        public MapTypeControlStyle MapTypeControlStyle { get; set; }

        public ControlPosition MapTypeControlPosition { get; set; }

        public bool MapTypeControlVisible { get; set; }

        public IList<Marker> Markers { get; private set; }

        public MarkerClusteringOptions MarkerClusteringOptions { get; private set; }

        public ControlPosition PanControlPosition { get; set; }

        public bool PanControlVisible { get; set; }

        public bool OverviewMapControlVisible { get; set; }

        public bool OverviewMapControlOpened { get; set; }

        public IList<Polygon> Polygons { get; private set; }

        public IList<Circle> Circles { get; private set; }

        public bool ScaleControlVisible { get; set; }

        public bool StreetViewControlVisible { get; set; }

        public ControlPosition StreetViewControlPosition { get; set; }

        public int Width { get; set; }

        public int Zoom { get; set; }

        public bool ZoomControlVisible { get; set; }

        public ControlPosition ZoomControlPosition { get; set; }

        public ZoomControlStyle ZoomControlStyle { get; set; }

        public ViewContext ViewContext
        {
            get;
            private set;
        }

        #endregion

        #region Constructor

        public GoogleMap(ViewContext viewContext)
        {
            ScriptFileNames = new List<string>();
            ScriptFileNames.AddRange(new[] { "jmelosegui.googlemap.js" });

            ClientEvents = new GoogleMapClientEvents();

            if (viewContext == null)
            {
                throw new ArgumentNullException("viewContext");
            }

            ViewContext = viewContext;
            Initialize();
        }

        public List<string> ScriptFileNames { get; private set; }

        private void Initialize()
        {
            ClientEvents = new GoogleMapClientEvents();
            DisableDoubleClickZoom = false;
            Draggable = true;
            EnableMarkersClustering = false;
            Latitude = 23;
            Longitude = -82;
            MapType = MapType.Roadmap;
            MapTypeControlPosition = ControlPosition.TopRight;
            MapTypeControlVisible = true;
            Markers = new List<Marker>();
            MarkerClusteringOptions = new MarkerClusteringOptions();
            Polygons = new List<Polygon>();
            Circles = new List<Circle>();
            PanControlPosition = ControlPosition.TopLeft;
            PanControlVisible = true;
            OverviewMapControlVisible = false;
            OverviewMapControlOpened = false;
            StreetViewControlVisible = true;
            StreetViewControlPosition = ControlPosition.TopLeft;
            ZoomControlVisible = true;
            ZoomControlPosition = ControlPosition.TopLeft;
            ZoomControlStyle = ZoomControlStyle.Default;
            ScaleControlVisible = false;
            Height = 300;
            Width = 0;            
        }

        #endregion

        #region Override Methods

        public virtual void WriteInitializationScript(TextWriter writer)
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            var objectWriter = new ClientSideObjectWriter(Id, "GoogleMap", writer);

            objectWriter.Start();

            objectWriter.Append("ClientID", Id);
            objectWriter.Append("DisableDoubleClickZoom", DisableDoubleClickZoom, false);
            objectWriter.Append("Draggable", Draggable, true);
            objectWriter.Append("EnableMarkersClustering", EnableMarkersClustering, false);
            objectWriter.Append("Height", Height);
            objectWriter.Append("Latitude", Latitude);
            objectWriter.Append("Longitude", Longitude);
            objectWriter.Append("MapType", MapType, MapType.Roadmap);
            objectWriter.Append("MapTypeControlPosition", MapTypeControlPosition, ControlPosition.TopRight);
            objectWriter.Append("MapTypeControlVisible", MapTypeControlVisible, true);
            objectWriter.Append("MapTypeControlStyle", MapTypeControlStyle, MapTypeControlStyle.Default);
            objectWriter.Append("PanControlPosition", PanControlPosition, ControlPosition.TopLeft);
            objectWriter.Append("PanControlVisible", PanControlVisible, true);

            objectWriter.Append("OverviewMapControlVisible", OverviewMapControlVisible, false);
            objectWriter.Append("OverviewMapControlOpened", OverviewMapControlOpened, false);

            objectWriter.Append("StreetViewControlVisible", StreetViewControlVisible, true);
            objectWriter.Append("StreetViewControlPosition", StreetViewControlPosition, ControlPosition.TopLeft);

            objectWriter.Append("ZoomControlVisible", ZoomControlVisible, true);
            objectWriter.Append("ZoomControlPosition", ZoomControlPosition, ControlPosition.TopLeft);
            objectWriter.Append("ZoomControlStyle", ZoomControlStyle, ZoomControlStyle.Default);


            objectWriter.Append("ScaleControlVisible", ScaleControlVisible, false);
            objectWriter.Append("Width", Width, 0);
            objectWriter.Append("Zoom", (Zoom == 0) ? 6 : Zoom, 6);

            if (EnableMarkersClustering)
            {
                objectWriter.AppendObject("MarkerClusteringOptions", MarkerClusteringOptions.Serialize());
            }

            if (Markers.Any())
            {
                var markers = new List<IDictionary<string, object>>();

                Markers.Each(m => markers.Add(m.CreateSerializer().Serialize()));

                if (markers.Any())
                {
                    objectWriter.AppendCollection("Markers", markers);
                }
            }

            if (Polygons.Any())
            {
                var polygons = new List<IDictionary<string, object>>();

                Polygons.Each(p => polygons.Add(p.CreateSerializer().Serialize()));

                if (polygons.Any())
                {
                    objectWriter.AppendCollection("Polygons", polygons);
                }
            }

            if (Circles.Any())
            {
                var circles = new List<IDictionary<string, object>>();

                Circles.Each(c => circles.Add(c.CreateSerializer().Serialize()));

                if (circles.Any())
                {
                    objectWriter.AppendCollection("Circles", circles);
                }
            }

            this.ClientEvents.SerializeTo(objectWriter);

            objectWriter.Complete();

            Thread.CurrentThread.CurrentCulture = currentCulture;
        }

        protected virtual void WriteHtml(HtmlTextWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");

            var builder = new GoogleMapBuilder(this);
            IHtmlNode rootTag = builder.Build();
            rootTag.WriteTo(writer);

            var languaje = (Culture != null) ? "&language=" + Culture.TwoLetterISOLanguageName : String.Empty;
            var key = (ApiKey.HasValue()) ? "&key=" + ApiKey : String.Empty;
            
            var mainJs = String.Format("https://maps.googleapis.com/maps/api/js?v=3.exp{0}{1}", key, languaje);
            ScriptFileNames.Add(mainJs);

            if (EnableMarkersClustering)
                ScriptFileNames.Add("markerclusterer.js");

            if (Markers.Any(m => m.Window != null))
            {
                //Build Container for InfoWindows
                IHtmlNode infoWindowsRootTag = new HtmlElement("div")
                    .Attribute("id", String.Format("{0}-InfoWindowsHolder", Id))
                    .Attribute("style", "display: none");

                Markers.Where(m => m.Window != null).Each(m =>
                {
                    IHtmlNode markerInfoWindows = new HtmlElement("div")
                        .Attribute("id", String.Format("{0}Marker{1}", Id, m.Index))
                        .AddClass("content");
                    
                    m.Window.Template.Apply(markerInfoWindows);
                    infoWindowsRootTag.Children.Add(markerInfoWindows);
                });

                infoWindowsRootTag.WriteTo(writer);
            }
        }

        #endregion

        public virtual void BindTo<TGoogleMapOverlay, TDataItem>(IEnumerable<TDataItem> dataSource, Action<OverlayBindingFactory<TGoogleMapOverlay>> action)
            where TGoogleMapOverlay : Overlay
        {
            if (action == null) throw new ArgumentNullException("action");
            
            var factory = new OverlayBindingFactory<TGoogleMapOverlay>();
            action(factory);

            foreach (TDataItem dataItem in dataSource)
            {
                Overlay overlay = null;

                switch (typeof(TGoogleMapOverlay).FullName)
                {
                    case "Jmelosegui.Mvc.Googlemap.Overlays.Marker":
                        overlay = new Marker(this);
                        Markers.Add((Marker) overlay);
                        break;
                    case "Jmelosegui.Mvc.Googlemap.Overlays.Circle":
                        overlay = new Circle(this);
                        Circles.Add((Circle) overlay);
                        break;
                    case "Jmelosegui.Mvc.Googlemap.Overlays.Polygon":
                        overlay = new Polygon(this);
                        Polygons.Add((Polygon) overlay);
                        break;
                }

                factory.Binder.ItemDataBound((TGoogleMapOverlay)overlay, dataItem);
            }
        }

        public void Render()
        {
            TextWriter writer = ViewContext.Writer;
            using (HtmlTextWriter htmlTextWriter = new HtmlTextWriter(writer))
            {
                this.WriteHtml(htmlTextWriter);
            }
        }

        public string ToHtmlString()
        {
            string result;
            using (StringWriter stringWriter = new StringWriter())
            {
                this.WriteHtml(new HtmlTextWriter(stringWriter));
                result = stringWriter.ToString();
            }
            return result;
        }
    }

}
