using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using PixelLab_Desktop.Helpers;
using PixelLab_Desktop.ViewModels;

namespace PixelLab_Desktop.Views
{
    public partial class ColorSpaceSidebar : UserControl
    {
        private ColorSidebarViewModel _vm = new ColorSidebarViewModel();
        private bool _isInitialized = false;


        // Which 2D image is currently shown (for click→color mapping)
        private string _activeSpace = "RGB";

        // Cached bitmaps for the 2D panels
        private WriteableBitmap? _bmpCmyk, _bmpYuv, _bmpYCbCr, _bmpLab;

        public ColorSpaceSidebar()
        {
            InitializeComponent();
            _isInitialized = true;
            InitWebView();
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () =>
            {
                ShowSpace("RGB");
            });
        }

        // ── WebView2 initialization ──────────────────────────────────────
        private async void InitWebView()
        {
            try
            {
                await WebView3D.EnsureCoreWebView2Async(null);
                WebView3D.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
                LoadThreeScene("RGB");
            }
            catch
            {
                // WebView2 runtime not installed — fall back gracefully
            }
        }

        // ── Space selector buttons ───────────────────────────────────────
        private void SpaceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string space)
                ShowSpace(space);
        }

        private void ShowSpace(string space)
        {
            _activeSpace = space;
            _vm.ActiveSpace = space;

            // Hide all panels
            WebView3D.Visibility  = Visibility.Collapsed;
            PanelCmyk.Visibility  = Visibility.Collapsed;
            PanelYuv.Visibility   = Visibility.Collapsed;
            PanelYCbCr.Visibility = Visibility.Collapsed;
            PanelLab.Visibility   = Visibility.Collapsed;

            switch (space)
            {
                case "RGB":
                case "HSV":
                    WebView3D.Visibility = Visibility.Visible;
                    LoadThreeScene(space);
                    break;

                case "CMYK":
                    PanelCmyk.Visibility = Visibility.Visible;
                    RedrawCmyk();
                    break;

                case "YUV":
                    PanelYuv.Visibility = Visibility.Visible;
                    RedrawYuv();
                    break;

                case "YCbCr":
                    PanelYCbCr.Visibility = Visibility.Visible;
                    RedrawYCbCr();
                    break;

                case "LAB":
                    PanelLab.Visibility = Visibility.Visible;
                    RedrawLab();
                    break;
            }
        }

        // ── Three.js scene loader ────────────────────────────────────────
        private void LoadThreeScene(string space)
        {
            if (WebView3D.CoreWebView2 == null) return;

            string html = space == "RGB" ? BuildRgbCubeHtml() : BuildHsvCylinderHtml();
            WebView3D.NavigateToString(html);
        }

        // ── Receive color picks from the Three.js scene ──────────────────
        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            // Message format: "r,g,b"  e.g. "255,128,0"
            var parts = e.TryGetWebMessageAsString().Split(',');
            if (parts.Length == 3 &&
                byte.TryParse(parts[0], out byte r) &&
                byte.TryParse(parts[1], out byte g) &&
                byte.TryParse(parts[2], out byte b))
            {
                Dispatcher.Invoke(() => SetPickedColor(r, g, b));
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowSpace(_activeSpace);
        }

        // ── 2D bitmap redraws ────────────────────────────────────────────
        private void RedrawCmyk()
        {
            if (!_isInitialized) return;
            int w = ImgCmyk.ActualWidth > 0 ? (int)ImgCmyk.ActualWidth : 280;
            int h = ImgCmyk.ActualHeight > 0 ? (int)ImgCmyk.ActualHeight : 200;
            _bmpCmyk = ColorSpaceVisualizer.DrawCmykBars(w, h);
            ImgCmyk.Source = _bmpCmyk;
        }

        private void RedrawYuv()
        {
            if (!_isInitialized) return;
            int w = ImgYuv.ActualWidth > 0 ? (int)ImgYuv.ActualWidth : 280;
            int h = ImgYuv.ActualHeight > 0 ? (int)ImgYuv.ActualHeight : 180;
            _bmpYuv = ColorSpaceVisualizer.DrawYuvPlane(w, h, (float)SliderYuvY.Value);
            ImgYuv.Source = _bmpYuv;
            TxtYuvY.Text = $"Y = {(int)(SliderYuvY.Value * 100)}%";
        }

        private void RedrawYCbCr()
        {
            if (!_isInitialized) return;
            int w = ImgYCbCr.ActualWidth > 0 ? (int)ImgYCbCr.ActualWidth : 280;
            int h = ImgYCbCr.ActualHeight > 0 ? (int)ImgYCbCr.ActualHeight : 180;
            _bmpYCbCr = ColorSpaceVisualizer.DrawYCbCrPlane(w, h, (float)SliderYCbCrY.Value);
            ImgYCbCr.Source = _bmpYCbCr;
            TxtYCbCrY.Text = $"Y = {(int)(SliderYCbCrY.Value * 100)}%";
        }

        private void RedrawLab()
        {
            if (!_isInitialized) return;
            int w = ImgLab.ActualWidth > 0 ? (int)ImgLab.ActualWidth : 280;
            int h = ImgLab.ActualHeight > 0 ? (int)ImgLab.ActualHeight : 180;
            _bmpLab = ColorSpaceVisualizer.DrawLabPlane(w, h, (float)SliderLabL.Value);
            ImgLab.Source = _bmpLab;
            TxtLabL.Text = $"L = {(int)(SliderLabL.Value * 100)}";
        }

        // ── Slider change handlers ───────────────────────────────────────
        private void YuvSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)   => RedrawYuv();
        private void YCbCrSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) => RedrawYCbCr();
        private void LabSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)   => RedrawLab();

        // ── 2D plane mouse click / drag ──────────────────────────────────
        private void Plane_MouseDown(object sender, MouseButtonEventArgs e) => PickFromPlane(sender, e.GetPosition((Image)sender));
        private void Plane_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                PickFromPlane(sender, e.GetPosition((Image)sender));
        }

        private void PickFromPlane(object sender, Point pos)
        {
            Image img = (Image)sender;
            WriteableBitmap? bmp = _activeSpace switch
            {
                "CMYK"  => _bmpCmyk,
                "YUV"   => _bmpYuv,
                "YCbCr" => _bmpYCbCr,
                "LAB"   => _bmpLab,
                _       => null
            };
            if (bmp == null) return;

            // Map Image control coords → bitmap pixel coords
            int px = (int)(pos.X / img.ActualWidth  * bmp.PixelWidth);
            int py = (int)(pos.Y / img.ActualHeight * bmp.PixelHeight);
            px = Math.Clamp(px, 0, bmp.PixelWidth  - 1);
            py = Math.Clamp(py, 0, bmp.PixelHeight - 1);

            // Read pixel (BGRA format)
            byte[] pixel = new byte[4];
            bmp.CopyPixels(new Int32Rect(px, py, 1, 1), pixel, 4, 0);
            SetPickedColor(pixel[2], pixel[1], pixel[0]);
        }

        // ── Update color info panel ──────────────────────────────────────
        private void SetPickedColor(byte r, byte g, byte b)
        {
            _vm.PickedColor = Color.FromRgb(r, g, b);

            // Update swatch
            ColorSwatch.Background = new SolidColorBrush(Color.FromRgb(r, g, b));

            // Update all text rows
            TxtRgb.Text   = _vm.RgbText;
            TxtHsv.Text   = _vm.HsvText;
            TxtCmyk.Text  = _vm.CmykText;
            TxtYuv.Text   = _vm.YuvText;
            TxtYCbCr.Text = _vm.YCbCrText;
            TxtLab.Text   = _vm.LabText;
        }

        // ════════════════════════════════════════════════════════════════
        // Three.js HTML builders
        // ════════════════════════════════════════════════════════════════

        private static string BuildRgbCubeHtml() => @"
<!DOCTYPE html><html><body style='margin:0;background:#1a1a1a;overflow:hidden'>
<canvas id='c' style='display:block'></canvas>
<script src='https://cdnjs.cloudflare.com/ajax/libs/three.js/r128/three.min.js'></script>
<script>
const W = window.innerWidth, H = window.innerHeight;
const renderer = new THREE.WebGLRenderer({canvas:document.getElementById('c'),antialias:true});
renderer.setSize(W,H);
const scene = new THREE.Scene();
scene.background = new THREE.Color(0x1a1a1a);
const camera = new THREE.PerspectiveCamera(45, W/H, 0.1, 100);
camera.position.set(2,2,2);
camera.lookAt(0,0,0);

// RGB Cube: each vertex colored by its RGB position
const geo = new THREE.BoxGeometry(1,1,1);
const colors = [];
const pos = geo.attributes.position;
for(let i=0;i<pos.count;i++){
  const x=pos.getX(i)+0.5, y=pos.getY(i)+0.5, z=pos.getZ(i)+0.5;
  colors.push(x,y,z);
}
geo.setAttribute('color',new THREE.Float32BufferAttribute(colors,3));
const mat = new THREE.MeshBasicMaterial({vertexColors:true,side:THREE.DoubleSide});
const cube = new THREE.Mesh(geo,mat);
scene.add(cube);

// Edges
const edges = new THREE.EdgesGeometry(new THREE.BoxGeometry(1,1,1));
scene.add(new THREE.LineSegments(edges, new THREE.LineBasicMaterial({color:0xffffff,opacity:0.3,transparent:true})));

// Axis labels hint
const axisColors = [{c:0xff0000,v:new THREE.Vector3(0.7,0,0)},{c:0x00ff00,v:new THREE.Vector3(0,0.7,0)},{c:0x0000ff,v:new THREE.Vector3(0,0,0.7)}];
axisColors.forEach(a=>{const g=new THREE.SphereGeometry(0.025);scene.add(new THREE.Mesh(g,new THREE.MeshBasicMaterial({color:a.c})));});

// Mouse rotate
let isDragging=false,lastX=0,lastY=0;
document.addEventListener('mousedown',e=>{isDragging=true;lastX=e.clientX;lastY=e.clientY;});
document.addEventListener('mouseup',  ()=>{isDragging=false;});
document.addEventListener('mousemove',e=>{
  if(!isDragging)return;
  cube.rotation.y+=(e.clientX-lastX)*0.01;
  cube.rotation.x+=(e.clientY-lastY)*0.01;
  lastX=e.clientX;lastY=e.clientY;
});
// Zoom
document.addEventListener('wheel',e=>{camera.position.multiplyScalar(e.deltaY>0?1.1:0.9);});

// Click to pick color
document.addEventListener('click',e=>{
  const rect=renderer.domElement.getBoundingClientRect();
  const mouse=new THREE.Vector2(
    ((e.clientX-rect.left)/rect.width)*2-1,
   -((e.clientY-rect.top)/rect.height)*2+1
  );
  const ray=new THREE.Raycaster();
  ray.setFromCamera(mouse,camera);
  const hits=ray.intersectObject(cube);
  if(hits.length>0){
    const p=hits[0].point;
    const r=Math.round(Math.clamp01(p.x+0.5)*255);
    const g=Math.round(Math.clamp01(p.y+0.5)*255);
    const b=Math.round(Math.clamp01(p.z+0.5)*255);
    window.chrome.webview.postMessage(r+','+g+','+b);
  }
});
THREE.MathUtils.clamp01=(v)=>Math.max(0,Math.min(1,v));
Math.clamp01=(v)=>Math.max(0,Math.min(1,v));

function animate(){requestAnimationFrame(animate);renderer.render(scene,camera);}
animate();
</script></body></html>";

        private static string BuildHsvCylinderHtml() => @"
<!DOCTYPE html><html><body style='margin:0;background:#1a1a1a;overflow:hidden'>
<canvas id='c' style='display:block'></canvas>
<script src='https://cdnjs.cloudflare.com/ajax/libs/three.js/r128/three.min.js'></script>
<script>
const W=window.innerWidth,H=window.innerHeight;
const renderer=new THREE.WebGLRenderer({canvas:document.getElementById('c'),antialias:true});
renderer.setSize(W,H);
const scene=new THREE.Scene();
scene.background=new THREE.Color(0x1a1a1a);
const camera=new THREE.PerspectiveCamera(45,W/H,0.1,100);
camera.position.set(0,2.5,3);
camera.lookAt(0,0,0);

function hsvToRgb(h,s,v){
  const i=Math.floor(h*6),f=h*6-i,p=v*(1-s),q=v*(1-f*s),t=v*(1-(1-f)*s);
  switch(i%6){
    case 0:return[v,t,p];case 1:return[q,v,p];case 2:return[p,v,t];
    case 3:return[p,q,v];case 4:return[t,p,v];case 5:return[v,p,q];
  }
  return[0,0,0];
}

// Build cylinder geometry with HSV colors
const segments=64, stacks=20, radius=1, height=2;
const geo=new THREE.CylinderGeometry(radius,radius,height,segments,stacks,true);
const colors=[];
const pos=geo.attributes.position;
for(let i=0;i<pos.count;i++){
  const x=pos.getX(i),y=pos.getY(i),z=pos.getZ(i);
  const h=(Math.atan2(z,x)/(2*Math.PI)+1)%1;
  const s=Math.sqrt(x*x+z*z)/radius;
  const v=(y/height)+0.5;
  const [r,g,b]=hsvToRgb(h,s,v);
  colors.push(r,g,b);
}
geo.setAttribute('color',new THREE.Float32BufferAttribute(colors,3));
const mat=new THREE.MeshBasicMaterial({vertexColors:true,side:THREE.DoubleSide});
const cyl=new THREE.Mesh(geo,mat);
scene.add(cyl);

// Top/bottom discs
[0.5,-0.5].forEach(yPos=>{
  const discGeo=new THREE.CircleGeometry(radius,segments);
  const discColors=[];
  const dpos=discGeo.attributes.position;
  for(let i=0;i<dpos.count;i++){
    const x=dpos.getX(i),z=dpos.getZ(i);
    const h=(Math.atan2(z,x)/(2*Math.PI)+1)%1;
    const s=Math.sqrt(x*x+z*z)/radius;
    const v=yPos>0?1:0;
    const [r,g,b]=hsvToRgb(h,Math.min(1,s),v);
    discColors.push(r,g,b);
  }
  discGeo.setAttribute('color',new THREE.Float32BufferAttribute(discColors,3));
  const disc=new THREE.Mesh(discGeo,new THREE.MeshBasicMaterial({vertexColors:true,side:THREE.DoubleSide}));
  disc.position.y=yPos*height;
  disc.rotation.x=yPos>0?0:Math.PI;
  scene.add(disc);
});

// Mouse rotate
let isDragging=false,lastX=0,lastY=0;
document.addEventListener('mousedown',e=>{isDragging=true;lastX=e.clientX;lastY=e.clientY;});
document.addEventListener('mouseup',()=>{isDragging=false;});
document.addEventListener('mousemove',e=>{
  if(!isDragging)return;
  cyl.rotation.y+=(e.clientX-lastX)*0.01;
  lastX=e.clientX;lastY=e.clientY;
});
document.addEventListener('wheel',e=>{camera.position.multiplyScalar(e.deltaY>0?1.1:0.9);});

// Click to pick
const raycaster=new THREE.Raycaster();
document.addEventListener('click',e=>{
  const rect=renderer.domElement.getBoundingClientRect();
  const mouse=new THREE.Vector2(
    ((e.clientX-rect.left)/rect.width)*2-1,
   -((e.clientY-rect.top)/rect.height)*2+1
  );
  raycaster.setFromCamera(mouse,camera);
  const hits=raycaster.intersectObject(cyl);
  if(hits.length>0){
    const p=hits[0].point;
    const h=((Math.atan2(p.z,p.x)/(2*Math.PI))+1)%1;
    const s=Math.min(1,Math.sqrt(p.x*p.x+p.z*p.z)/radius);
    const v=Math.max(0,Math.min(1,(p.y/height)+0.5));
    const [r,g,b]=hsvToRgb(h,s,v);
    window.chrome.webview.postMessage(
      Math.round(r*255)+','+Math.round(g*255)+','+Math.round(b*255)
    );
  }
});

function animate(){requestAnimationFrame(animate);renderer.render(scene,camera);}
animate();
</script></body></html>";
    }
}
