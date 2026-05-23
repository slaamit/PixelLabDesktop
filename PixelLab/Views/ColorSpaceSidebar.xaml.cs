using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using PixelLab_Desktop.ViewModels;

namespace PixelLab_Desktop.Views
{
    public partial class ColorSpaceSidebar : UserControl
    {
        private bool _isInitialized = false;
        private ColorSidebarViewModel _vm = new ColorSidebarViewModel();

        public ColorSpaceSidebar()
        {
            InitializeComponent();
            _isInitialized = true;
            InitWebView();
        }

        // ── WebView2 init ────────────────────────────────────────────────
        private async void InitWebView()
        {
            try
            {
                await WebView3D.EnsureCoreWebView2Async(null);
                WebView3D.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () =>
                {
                    ShowSpace("RGB");
                });
            }
            catch { /* WebView2 runtime not installed */ }
        }

        // ── Space selector buttons ───────────────────────────────────────
        private void SpaceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string space)
                ShowSpace(space);
        }

        private void ShowSpace(string space)
        {
            if (WebView3D.CoreWebView2 == null) return;
            WebView3D.NavigateToString(BuildHtml(space));
        }

        // ── Receive picked color from Three.js ───────────────────────────
        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var parts = e.TryGetWebMessageAsString().Split(',');
            if (parts.Length == 3 &&
                byte.TryParse(parts[0], out byte r) &&
                byte.TryParse(parts[1], out byte g) &&
                byte.TryParse(parts[2], out byte b))
            {
                Dispatcher.Invoke(() => SetPickedColor(r, g, b));
            }
        }

        private void SetPickedColor(byte r, byte g, byte b)
        {
            _vm.PickedColor = Color.FromRgb(r, g, b);
            ColorSwatch.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
            TxtRgb.Text = _vm.RgbText;
            TxtHsv.Text = _vm.HsvText;
            TxtCmyk.Text = _vm.CmykText;
            TxtYuv.Text = _vm.YuvText;
            TxtYCbCr.Text = _vm.YCbCrText;
            TxtLab.Text = _vm.LabText;
        }

        // ════════════════════════════════════════════════════════════════
        // HTML router
        // ════════════════════════════════════════════════════════════════
        private static string BuildHtml(string space) => space switch
        {
            "RGB" => BuildRgbCubeHtml(),
            "HSV" => BuildHsvConeHtml(),
            "CMYK" => BuildCmyCubeHtml(),
            "YUV" => BuildYuvCylinderHtml(),
            "YCbCr" => BuildYCbCrConeHtml(),
            "LAB" => BuildLabSphereHtml(),
            _ => "<html><body style='color:white'>Unknown</body></html>"
        };

        // ════════════════════════════════════════════════════════════════
        // Shared JS snippets
        // ════════════════════════════════════════════════════════════════

        // Pixel-color picking — reads the rendered pixel from the WebGL
        // canvas, so it is always correct after any rotation or zoom.
        private static string PickingScript => @"
renderer.domElement.addEventListener('click', function(e) {
    renderer.render(scene, camera);
    const rect = renderer.domElement.getBoundingClientRect();
    const x = Math.floor((e.clientX - rect.left) * window.devicePixelRatio);
    const y = Math.floor((renderer.domElement.height) - (e.clientY - rect.top) * window.devicePixelRatio);
    const buf = new Uint8Array(4);
    renderer.getContext().readPixels(x, y, 1, 1, 0x1908, 0x1401, buf);
    if (buf[3] > 10) {
        window.chrome.webview.postMessage(buf[0]+','+buf[1]+','+buf[2]);
    }
});
";

        // Full-axis mouse rotate (X and Y) + scroll zoom
        private static string ControlsScript => @"
let _drag = false, _lx = 0, _ly = 0;
document.addEventListener('mousedown', e => { if(e.button===0){_drag=true;_lx=e.clientX;_ly=e.clientY;} });
document.addEventListener('mouseup',   () => _drag = false);
document.addEventListener('mousemove', e => {
    if (!_drag) return;
    group.rotation.y += (e.clientX - _lx) * 0.01;
    group.rotation.x += (e.clientY - _ly) * 0.01;
    _lx = e.clientX; _ly = e.clientY;
});
document.addEventListener('wheel', e => {
    camera.position.multiplyScalar(e.deltaY > 0 ? 1.1 : 0.9);
    e.preventDefault();
}, {passive:false});
function animate() { requestAnimationFrame(animate); renderer.render(scene, camera); }
animate();
";

        private static string MakeRenderer(string bg = "0x1a1a1a") => $@"
const renderer = new THREE.WebGLRenderer({{
    canvas: document.getElementById('c'),
    antialias: true,
    preserveDrawingBuffer: true
}});
renderer.setSize(window.innerWidth, window.innerHeight);
renderer.setPixelRatio(window.devicePixelRatio);
const scene = new THREE.Scene();
scene.background = new THREE.Color({bg});
const group = new THREE.Group();
scene.add(group);
";

        // ════════════════════════════════════════════════════════════════
        // 1. RGB CUBE
        // ════════════════════════════════════════════════════════════════
        private static string BuildRgbCubeHtml() => $@"
<!DOCTYPE html><html><body style='margin:0;overflow:hidden'>
<canvas id='c'></canvas>
<script src='https://cdnjs.cloudflare.com/ajax/libs/three.js/r128/three.min.js'></script>
<script>
{MakeRenderer()}
const camera = new THREE.PerspectiveCamera(45, innerWidth/innerHeight, 0.1, 100);
camera.position.set(1.8, 1.4, 1.8); camera.lookAt(0,0,0);

// Subdivided box so interior faces show gradients properly
const geo = new THREE.BoxGeometry(1,1,1,10,10,10);
const cols = []; const pos = geo.attributes.position;
for (let i=0;i<pos.count;i++) {{
    cols.push(
        Math.max(0,Math.min(1, pos.getX(i)+0.5)),
        Math.max(0,Math.min(1, pos.getY(i)+0.5)),
        Math.max(0,Math.min(1, pos.getZ(i)+0.5))
    );
}}
geo.setAttribute('color', new THREE.Float32BufferAttribute(cols,3));
group.add(new THREE.Mesh(geo, new THREE.MeshBasicMaterial({{vertexColors:true,side:THREE.DoubleSide}})));
group.add(new THREE.LineSegments(
    new THREE.EdgesGeometry(new THREE.BoxGeometry(1,1,1)),
    new THREE.LineBasicMaterial({{color:0xffffff,opacity:0.3,transparent:true}})
));
{PickingScript}
{ControlsScript}
</script></body></html>";

        // ════════════════════════════════════════════════════════════════
        // 2. HSV CONE  (point = black at bottom, wide top = full sat)
        // ════════════════════════════════════════════════════════════════
        private static string BuildHsvConeHtml() => $@"
<!DOCTYPE html><html><body style='margin:0;overflow:hidden'>
<canvas id='c'></canvas>
<script src='https://cdnjs.cloudflare.com/ajax/libs/three.js/r128/three.min.js'></script>
<script>
{MakeRenderer()}
const camera = new THREE.PerspectiveCamera(45, innerWidth/innerHeight, 0.1, 100);
camera.position.set(0, 1.8, 3.5); camera.lookAt(0,0,0);

function hsvToRgb(h,s,v) {{
    const i=Math.floor(h*6),f=h*6-i,p=v*(1-s),q=v*(1-f*s),t=v*(1-(1-f)*s);
    switch(i%6){{case 0:return[v,t,p];case 1:return[q,v,p];case 2:return[p,v,t];
                case 3:return[p,q,v];case 4:return[t,p,v];case 5:return[v,p,q];}}
    return[0,0,0];
}}

// Cone: radiusTop=0 (tip=black), radiusBottom=1 (base=full saturation)
// We flip it so tip is at bottom (y=-1) and base at top (y=+1)
const geo = new THREE.ConeGeometry(1, 2, 128, 32, true);
geo.rotateZ(Math.PI); // flip: tip now at bottom
const cols=[], pos=geo.attributes.position;
for(let i=0;i<pos.count;i++) {{
    const x=pos.getX(i), y=pos.getY(i), z=pos.getZ(i);
    const h = ((Math.atan2(z,x)/(2*Math.PI))+1)%1;
    // v: 0 at tip (y=+1 after flip becomes y=-1... recompute from geometry)
    // After rotateZ(PI): original tip (y=+1) → y=-1, base(y=-1)→y=+1
    const vNorm = Math.max(0, Math.min(1, (y+1)/2)); // 0=tip(black), 1=base(bright)
    const coneR = vNorm; // radius of cone at this height
    const dist = Math.sqrt(x*x+z*z);
    const s = coneR > 0.001 ? Math.min(1, dist/coneR) : 0;
    const [r,g,b] = hsvToRgb(h, s, vNorm);
    cols.push(r,g,b);
}}
geo.setAttribute('color', new THREE.Float32BufferAttribute(cols,3));
group.add(new THREE.Mesh(geo, new THREE.MeshBasicMaterial({{vertexColors:true,side:THREE.DoubleSide}})));

// Top disc (base of cone, V=1)
const dGeo = new THREE.CircleGeometry(1,128);
const dCols=[], dp=dGeo.attributes.position;
for(let i=0;i<dp.count;i++) {{
    const x=dp.getX(i), z=dp.getZ(i);
    const h=((Math.atan2(z,x)/(2*Math.PI))+1)%1;
    const s=Math.min(1,Math.sqrt(x*x+z*z));
    const [r,g,b]=hsvToRgb(h,s,1);
    dCols.push(r,g,b);
}}
dGeo.setAttribute('color', new THREE.Float32BufferAttribute(dCols,3));
const disc = new THREE.Mesh(dGeo, new THREE.MeshBasicMaterial({{vertexColors:true,side:THREE.DoubleSide}}));
disc.position.y = 1; disc.rotation.x = -Math.PI/2;
group.add(disc);

{PickingScript}
{ControlsScript}
</script></body></html>";

        // ════════════════════════════════════════════════════════════════
        // 3. CMY CUBE  (axes: C=+X, M=+Y, Y=+Z; corners match image 3)
        // ════════════════════════════════════════════════════════════════
        private static string BuildCmyCubeHtml() => $@"
<!DOCTYPE html><html><body style='margin:0;overflow:hidden'>
<canvas id='c'></canvas>
<script src='https://cdnjs.cloudflare.com/ajax/libs/three.js/r128/three.min.js'></script>
<script>
{MakeRenderer()}
const camera = new THREE.PerspectiveCamera(45, innerWidth/innerHeight, 0.1, 100);
// Match reference: Magenta top-left, Cyan right, Yellow bottom-left
camera.position.set(-1.6, 1.6, 2.2); camera.lookAt(0,0,0);

const geo = new THREE.BoxGeometry(1,1,1,10,10,10);
const cols=[], pos=geo.attributes.position;
for(let i=0;i<pos.count;i++) {{
    const C=Math.max(0,Math.min(1,pos.getX(i)+0.5));
    const M=Math.max(0,Math.min(1,pos.getY(i)+0.5));
    const Y=Math.max(0,Math.min(1,pos.getZ(i)+0.5));
    cols.push(1-C, 1-M, 1-Y); // RGB = (1-C, 1-M, 1-Y)
}}
geo.setAttribute('color', new THREE.Float32BufferAttribute(cols,3));
group.add(new THREE.Mesh(geo, new THREE.MeshBasicMaterial({{vertexColors:true,side:THREE.DoubleSide}})));
group.add(new THREE.LineSegments(
    new THREE.EdgesGeometry(new THREE.BoxGeometry(1,1,1)),
    new THREE.LineBasicMaterial({{color:0xffffff,opacity:0.3,transparent:true}})
));
{PickingScript}
{ControlsScript}
</script></body></html>";

        // ════════════════════════════════════════════════════════════════
        // 4. YUV CYLINDER  (Y = vertical axis, U/V = chrominance plane)
        // ════════════════════════════════════════════════════════════════
        private static string BuildYuvCylinderHtml() => $@"
<!DOCTYPE html><html><body style='margin:0;overflow:hidden'>
<canvas id='c'></canvas>
<script src='https://cdnjs.cloudflare.com/ajax/libs/three.js/r128/three.min.js'></script>
<script>
{MakeRenderer()}
const camera = new THREE.PerspectiveCamera(45, innerWidth/innerHeight, 0.1, 100);
camera.position.set(0, 2, 4); camera.lookAt(0,0,0);

function yuvToRgb(Y,U,V) {{
    const r = Y + 1.13983*V;
    const g = Y - 0.39465*U - 0.58060*V;
    const b = Y + 2.03211*U;
    return [Math.max(0,Math.min(255,r))/255,
            Math.max(0,Math.min(255,g))/255,
            Math.max(0,Math.min(255,b))/255];
}}

// Cylinder body: Y axis = vertical, U/V = angular + radial
const geo = new THREE.CylinderGeometry(1,1,2,128,32,true);
const cols=[], pos=geo.attributes.position;
for(let i=0;i<pos.count;i++) {{
    const x=pos.getX(i), y=pos.getY(i), z=pos.getZ(i);
    const Yv = Math.max(0, Math.min(255, (y/2+0.5)*255));
    const angle = Math.atan2(z,x);
    const dist = Math.min(1, Math.sqrt(x*x+z*z));
    const U = dist * 0.436 * Math.cos(angle) * 255;  // U range ≈ ±111
    const V = dist * 0.615 * Math.sin(angle) * 255;  // V range ≈ ±157
    const [r,g,b] = yuvToRgb(Yv, U, V);
    cols.push(r,g,b);
}}
geo.setAttribute('color', new THREE.Float32BufferAttribute(cols,3));
group.add(new THREE.Mesh(geo, new THREE.MeshBasicMaterial({{vertexColors:true,side:THREE.DoubleSide}})));

// Top disc (Y=255) and Bottom disc (Y=0)
[1,-1].forEach(sign => {{
    const dGeo = new THREE.CircleGeometry(1,128);
    const dCols=[], dp=dGeo.attributes.position;
    const Yv = sign > 0 ? 255 : 0;
    for(let i=0;i<dp.count;i++) {{
        const x=dp.getX(i), z=dp.getZ(i);
        const angle=Math.atan2(z,x);
        const dist=Math.min(1,Math.sqrt(x*x+z*z));
        const U=dist*0.436*Math.cos(angle)*255;
        const V=dist*0.615*Math.sin(angle)*255;
        const [r,g,b]=yuvToRgb(Yv,U,V);
        dCols.push(r,g,b);
    }}
    dGeo.setAttribute('color',new THREE.Float32BufferAttribute(dCols,3));
    const d=new THREE.Mesh(dGeo,new THREE.MeshBasicMaterial({{vertexColors:true,side:THREE.DoubleSide}}));
    d.position.y=sign; d.rotation.x = sign>0 ? -Math.PI/2 : Math.PI/2;
    group.add(d);
}});
{PickingScript}
{ControlsScript}
</script></body></html>";

        // ════════════════════════════════════════════════════════════════
        // 5. YCbCr CONE  (matches image 1 — same shape as HSV cone)
        // ════════════════════════════════════════════════════════════════
        private static string BuildYCbCrConeHtml() => $@"
<!DOCTYPE html><html><body style='margin:0;overflow:hidden'>
<canvas id='c'></canvas>
<script src='https://cdnjs.cloudflare.com/ajax/libs/three.js/r128/three.min.js'></script>
<script>
{MakeRenderer()}
const camera = new THREE.PerspectiveCamera(45, innerWidth/innerHeight, 0.1, 100);
camera.position.set(0, 1.8, 3.5); camera.lookAt(0,0,0);

function ycbcrToRgb(Y,Cb,Cr) {{
    const r=Y+1.402*(Cr-128);
    const g=Y-0.344136*(Cb-128)-0.714136*(Cr-128);
    const b=Y+1.772*(Cb-128);
    return [Math.max(0,Math.min(255,r))/255,
            Math.max(0,Math.min(255,g))/255,
            Math.max(0,Math.min(255,b))/255];
}}

const geo = new THREE.ConeGeometry(1,2,128,32,true);
geo.rotateZ(Math.PI); // tip at bottom
const cols=[], pos=geo.attributes.position;
for(let i=0;i<pos.count;i++) {{
    const x=pos.getX(i),y=pos.getY(i),z=pos.getZ(i);
    const vNorm=Math.max(0,Math.min(1,(y+1)/2));
    const Y=vNorm*235;  // luma 0→235
    const coneR=vNorm;
    const dist=Math.sqrt(x*x+z*z);
    const satNorm=coneR>0.001?Math.min(1,dist/coneR):0;
    const angle=Math.atan2(z,x);
    const Cb=128+satNorm*112*Math.cos(angle);
    const Cr=128+satNorm*112*Math.sin(angle);
    const [r,g,b]=ycbcrToRgb(Y,Cb,Cr);
    cols.push(r,g,b);
}}
geo.setAttribute('color',new THREE.Float32BufferAttribute(cols,3));
group.add(new THREE.Mesh(geo,new THREE.MeshBasicMaterial({{vertexColors:true,side:THREE.DoubleSide}})));

// Top disc (Y=235, full chroma)
const dGeo=new THREE.CircleGeometry(1,128);
const dCols=[], dp=dGeo.attributes.position;
for(let i=0;i<dp.count;i++) {{
    const x=dp.getX(i),z=dp.getZ(i);
    const dist=Math.min(1,Math.sqrt(x*x+z*z));
    const angle=Math.atan2(z,x);
    const Cb=128+dist*112*Math.cos(angle);
    const Cr=128+dist*112*Math.sin(angle);
    const [r,g,b]=ycbcrToRgb(235,Cb,Cr);
    dCols.push(r,g,b);
}}
dGeo.setAttribute('color',new THREE.Float32BufferAttribute(dCols,3));
const disc=new THREE.Mesh(dGeo,new THREE.MeshBasicMaterial({{vertexColors:true,side:THREE.DoubleSide}}));
disc.position.y=1; disc.rotation.x=-Math.PI/2;
group.add(disc);

{PickingScript}
{ControlsScript}
</script></body></html>";

        // ════════════════════════════════════════════════════════════════
        // 6. LAB SPHERE  (matches image 2)
        //    L = vertical (black bottom → white top)
        //    a* = left/right (green ← → red)
        //    b* = front/back (blue ← → yellow)
        // ════════════════════════════════════════════════════════════════
        private static string BuildLabSphereHtml() => $@"
<!DOCTYPE html><html><body style='margin:0;overflow:hidden'>
<canvas id='c'></canvas>
<script src='https://cdnjs.cloudflare.com/ajax/libs/three.js/r128/three.min.js'></script>
<script>
{MakeRenderer()}
const camera = new THREE.PerspectiveCamera(45, innerWidth/innerHeight, 0.1, 100);
camera.position.set(0.5, 2.5, 4); camera.lookAt(0,0,0);

function labToRgb(L,a,b) {{
    const fy=(L+16)/116, fx=a/500+fy, fz=fy-b/200;
    const f=t=>t>0.206893?t*t*t:(t-16/116)/7.787;
    const X=f(fx)*0.95047, Y=f(fy)*1.0, Z=f(fz)*1.08883;
    let r= X*3.2404542-Y*1.5371385-Z*0.4985314;
    let g=-X*0.9692660+Y*1.8760108+Z*0.0415560;
    let bv= X*0.0556434-Y*0.2040259+Z*1.0572252;
    const gc=v=>v>0.0031308?1.055*Math.pow(Math.max(0,v),1/2.4)-0.055:12.92*v;
    return [Math.max(0,Math.min(1,gc(r))),
            Math.max(0,Math.min(1,gc(g))),
            Math.max(0,Math.min(1,gc(bv)))];
}}

const R=1.5;
const geo=new THREE.SphereGeometry(R,64,64);
const cols=[], pos=geo.attributes.position;
for(let i=0;i<pos.count;i++) {{
    const x=pos.getX(i),y=pos.getY(i),z=pos.getZ(i);
    const L=Math.max(0,Math.min(100,(y/R+1)*50));
    const a=(x/R)*128;
    const bv=(z/R)*128;
    const [r,g,b]=labToRgb(L,a,bv);
    cols.push(r,g,b);
}}
geo.setAttribute('color',new THREE.Float32BufferAttribute(cols,3));
group.add(new THREE.Mesh(geo,new THREE.MeshBasicMaterial({{vertexColors:true,side:THREE.FrontSide}})));

// Equator reference ring
const ring=new THREE.TorusGeometry(R,0.015,8,128);
group.add(new THREE.Mesh(ring,new THREE.MeshBasicMaterial({{color:0xffffff,opacity:0.4,transparent:true}})));

{PickingScript}
{ControlsScript}
</script></body></html>";
    }
}