using System;
using System.Collections.Generic;
using System.Text;
using TgcViewer.Example;
using TgcViewer;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using TgcViewer.Utils.Modifiers;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.Sound;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSkeletalAnimation;
using TgcViewer.Utils.Terrain;
using System.Windows.Forms;
using TgcViewer.Utils._2D;

namespace AlumnoEjemplos.FriesPerSecond
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploAlumno : TgcExample
    {
        Device d3dDevice;

        //Meshes
        TgcBox piso;
        TgcSkyBox skyBox;
        TgcSkeletalMesh original;
        TgcMesh palmeraOriginal;
        List<TgcMesh> arboles;

        protected Point mouseCenter;

        //Mira
        TgcSprite mira;

        //Arma
        TgcSprite arma;


        //Musica
        //TgcMp3Player musicaFondo = GuiController.Instance.Mp3Player;
        string pathMusica;

        /// <summary>
        /// Categoría a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el árbol de la derecha de la pantalla.
        /// </summary>
        public override string getCategory()
        {
            return "AlumnoEjemplos";
        }

        /// <summary>
        /// Completar nombre del grupo en formato Grupo NN
        /// </summary>
        public override string getName()
        {
            return "FriesPerSecond";
        }

        /// <summary>
        /// Completar con la descripción del TP
        /// </summary>
        public override string getDescription()
        {
            return "MiIdea - Descripcion de la idea";
        }

        /// <summary>
        /// Método que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aquí todo el código de inicialización: cargar modelos, texturas, modifiers, uservars, etc.
        /// Borrar todo lo que no haga falta
        /// </summary>
        public override void init()
        {
            //GuiController.Instance: acceso principal a todas las herramientas del Framework

            //Device de DirectX para crear primitivas
            d3dDevice = GuiController.Instance.D3dDevice;

            //Carpeta de archivos Media del alumno
            string alumnoMediaFolder = GuiController.Instance.AlumnoEjemplosMediaDir;

            Control focusWindows = GuiController.Instance.D3dDevice.CreationParameters.FocusWindow;
            mouseCenter = focusWindows.PointToScreen(
                new Point(
                    focusWindows.Width / 2,
                    focusWindows.Height / 2)
                    );


            inicializarTerreno();

            pathMusica = GuiController.Instance.AlumnoEjemplosMediaDir + "Sonidos\\musica_fondo.mp3";
            GuiController.Instance.Mp3Player.closeFile();
            GuiController.Instance.Mp3Player.FileName = pathMusica;

            TgcMp3Player player = GuiController.Instance.Mp3Player;

            player.play(true);

            //Cargar malla original
            TgcSkeletalLoader loader = new TgcSkeletalLoader();
            string pathMesh = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml";
            string mediaPath = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\";
            original = loader.loadMeshFromFile(pathMesh, mediaPath);

            //Agregar animación a original
            loader.loadAnimationFromFile(original, mediaPath + "Parado-TgcSkeletalAnim.xml");

            //original.move(200, 0, 0);

            //Especificar la animación actual para todos los modelos
            original.playAnimation("Parado");

            //Crear Sprite
            mira = new TgcSprite();
            mira.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\mira.png");

            arma = new TgcSprite();
            arma.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\arma.png");



            //Ubicarlo centrado en la pantalla
            Size screenSize = GuiController.Instance.Panel3d.Size;
            Size textureSize = mira.Texture.Size;
            mira.Scaling = new Vector2(0.6f, 0.6f);
            mira.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - textureSize.Width / 2, 0), FastMath.Max(screenSize.Height / 2 - textureSize.Height / 2, 0));
            
            Size armaSize = arma.Texture.Size;
            float escalaAncho = (screenSize.Width / 2f) / armaSize.Width;

            arma.Scaling = new Vector2(escalaAncho, escalaAncho);
            arma.Position = new Vector2(screenSize.Width - (armaSize.Width * escalaAncho), screenSize.Height - (armaSize.Height * escalaAncho));

            //Cargar modelo de palmera original
            TgcSceneLoader loader1 = new TgcSceneLoader();
            TgcScene scene = loader1.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");
            palmeraOriginal = scene.Meshes[0];

            //Crear varias instancias del modelo original, pero sin volver a cargar el modelo entero cada vez
            int rows = 30;
            int cols = 10;
            float offset = 500;
            arboles = new List<TgcMesh>();
            //bool moverFila=false;

            Random rand = new Random();

            for (int i = 0; i < rows; i++)
            {
                
               
                for (int j = 0; j < cols; j++)
                {
                    //Crear instancia de modelo
                    TgcMesh instance = palmeraOriginal.createMeshInstance(palmeraOriginal.Name + i + "_" + j);


                    //Desplazarlo
                    instance.move(rand.Next(-5000,5000), 0, rand.Next(-5000,5000));
                    instance.Scale = new Vector3(1.3f, 1.3f, 1.3f);
                    arboles.Add(instance);
                }
            }

            ///////////////USER VARS//////////////////

            //Crear una UserVar
            GuiController.Instance.UserVars.addVar("variablePrueba");

            //Cargar valor en UserVar
            GuiController.Instance.UserVars.setValue("variablePrueba", 5451);



            ///////////////MODIFIERS//////////////////

            //Crear un modifier para un valor FLOAT
            GuiController.Instance.Modifiers.addFloat("valorFloat", -50f, 200f, 0f);

            //Crear un modifier para un ComboBox con opciones
            string[] opciones = new string[]{"opcion1", "opcion2", "opcion3"};
            GuiController.Instance.Modifiers.addInterval("valorIntervalo", opciones, 0);

            //Crear un modifier para modificar un vértice
            GuiController.Instance.Modifiers.addVertex3f("valorVertice", new Vector3(-100, -100, -100), new Vector3(50, 50, 50), new Vector3(0, 0, 0));



            ///////////////CONFIGURAR CAMARA ROTACIONAL//////////////////
            //Es la camara que viene por default, asi que no hace falta hacerlo siempre
            //GuiController.Instance.RotCamera.Enable = true;
            //Configurar centro al que se mira y distancia desde la que se mira
            //GuiController.Instance.RotCamera.setCamera(new Vector3(0, 0, 0), 300);
            Vector3 posicion = new Vector3();
            posicion = original.Position;
            posicion.Y += 120;
            posicion.Z += 10;
            GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0,120,0), new Vector3(1, 0, 0));
            //GuiController.Instance.FpsCamera.LookAt(new Vector3(0,120,0));
            GuiController.Instance.FpsCamera.JumpSpeed = 0;
            GuiController.Instance.FpsCamera.MovementSpeed *= 10;


            /*
            ///////////////CONFIGURAR CAMARA PRIMERA PERSONA//////////////////
            //Camara en primera persona, tipo videojuego FPS
            //Solo puede haber una camara habilitada a la vez. Al habilitar la camara FPS se deshabilita la camara rotacional
            //Por default la camara FPS viene desactivada
            GuiController.Instance.FpsCamera.Enable = true;
            //Configurar posicion y hacia donde se mira
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0, 0, -20), new Vector3(0, 0, 0));
            */
        }


        
        public override void render(float elapsedTime)
        {
            //Device de DirectX para renderizar
            d3dDevice = GuiController.Instance.D3dDevice;

            //Renderizar suelo
            piso.render();
            skyBox.render();

            //Renderizar original e instancias
            original.animateAndRender();

            //Renderizar instancias
            foreach (TgcMesh mesh in arboles)
            {
                mesh.render();
            }

            //Iniciar dibujado de todos los Sprites de la escena (en este caso es solo uno)
            GuiController.Instance.Drawer2D.beginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            mira.render();
            arma.render();

            //Finalizar el dibujado de Sprites
            GuiController.Instance.Drawer2D.endDrawSprite();

            //GuiController.Instance.FpsCamera.setCamera(GuiController.Instance.FpsCamera.Position, GuiController.Instance.FpsCamera.LookAt);
            ///////////////INPUT//////////////////
            //conviene deshabilitar ambas camaras para que no haya interferencia

        }

        /// <summary>
        /// Método que se llama cuando termina la ejecución del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {

        }

        public void inicializarTerreno()
        {
            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 500, 0);
            skyBox.Size = new Vector3(15000, 15000, 15000);
            string texturesPath = GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox4\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "city_top2.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "city_down.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "city_left.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "city_right.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "city_front.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "city_back.jpg");
            skyBox.SkyEpsilon =50f;
            skyBox.updateValues();

            //Crear piso
            TgcTexture pisoTexture = TgcTexture.createTexture(d3dDevice, GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\TexturePack3\\pasto.jpg");
            piso = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(10000, 0, 10000), pisoTexture);
        }

    }
}
