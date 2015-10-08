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
using TgcViewer.Utils.Input;
using TgcViewer.Utils.Shaders;

namespace AlumnoEjemplos.FriesPerSecond
{
    /// <summary>
    /// Ejemplo del alumno
    /// </summary>
    public class EjemploAlumno : TgcExample
    {
        Device d3dDevice;
        //Escena
        TgcScene scene;

        //Meshes
        TgcBox piso;
        TgcSkyBox skyBox;
        TgcSkeletalMesh original;
        List<TgcSkeletalMesh> instanciasMalos;
        TgcMesh palmeraOriginal;
        List<TgcMesh> arboles;
        TgcText2d vida;
        TgcBox lightMesh;

        Effect effect;
        float time;

        protected Point mouseCenter;

        //Mira
        TgcSprite mira;
        TgcSprite mira_zoom;

        //Arma
        TgcSprite arma;


        //Banderas
        bool miraActivada;

        //Variables
        float anguloFov;
        float aspectRatio;
        float velocidadAngular = 1.75f;
        float velocidadMov = 750f;
        float velocidadCorrer = 1500f;
        float ruedita;

        //Musica
        //TgcMp3Player musicaFondo = GuiController.Instance.Mp3Player;
        string pathMusica;

        //Camara
        Q3FpsCamera camaraQ3;

        /// <summary>
        /// Categor�a a la que pertenece el ejemplo.
        /// Influye en donde se va a haber en el �rbol de la derecha de la pantalla.
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
        /// Completar con la descripci�n del TP
        /// </summary>
        public override string getDescription()
        {
            return "MiIdea - Descripcion de la idea";
        }

        /// <summary>
        /// M�todo que se llama una sola vez,  al principio cuando se ejecuta el ejemplo.
        /// Escribir aqu� todo el c�digo de inicializaci�n: cargar modelos, texturas, modifiers, uservars, etc.
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

            pathMusica = GuiController.Instance.AlumnoEjemplosMediaDir + "Sonidos\\Stayin_alive.mp3";
            GuiController.Instance.Mp3Player.closeFile();
            GuiController.Instance.Mp3Player.FileName = pathMusica;

            TgcMp3Player player = GuiController.Instance.Mp3Player;

            player.play(true);

            //Cargar malla original
            TgcSkeletalLoader loader = new TgcSkeletalLoader();
            string pathMesh = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml";
            string mediaPath = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\Robot\\";
            original = loader.loadMeshFromFile(pathMesh, mediaPath);
            original.Scale = new Vector3(2.0f, 2.0f,2.0f);

            //Agregar animaci�n a original
            loader.loadAnimationFromFile(original, mediaPath + "Patear-TgcSkeletalAnim.xml");

            //original.move(200, 0, 0);

            float offset = 200;
            int cantInstancias = 4;
            instanciasMalos = new List<TgcSkeletalMesh>();
            for (int i = 0; i < cantInstancias; i++)
            {
                TgcSkeletalMesh robotito = original.createMeshInstance(original.Name + i);

                robotito.move(i * offset, 0, 0);
                instanciasMalos.Add(robotito);
            }

            //Especificar la animaci�n actual para todos los modelos
            original.playAnimation("Patear");
            foreach (TgcSkeletalMesh robot in instanciasMalos)
            {
                robot.playAnimation("Patear");
            }

            //Crear Sprite
            mira = new TgcSprite();
            mira.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\mira.png");
            miraActivada = false;
            mira_zoom = new TgcSprite();
            mira_zoom.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\mira_zoom3.png");

            arma = new TgcSprite();
            arma.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\arma.png");



            //Ubicarlo centrado en la pantalla
            Size screenSize = GuiController.Instance.Panel3d.Size;
            Size textureSize = mira.Texture.Size;
            mira.Scaling = new Vector2(0.6f, 0.6f);
            mira.Position = new Vector2(FastMath.Max(screenSize.Width / 2 - (textureSize.Width * 0.6f) / 2, 0), FastMath.Max(screenSize.Height / 2 - (textureSize.Height * 0.6f) / 2, 0));

            mira_zoom.Scaling = new Vector2((float)screenSize.Width / mira_zoom.Texture.Size.Width, (float)screenSize.Height / mira_zoom.Texture.Size.Height);
            mira_zoom.Position = new Vector2(0,0);

            Size armaSize = arma.Texture.Size;
            float escalaAncho = (screenSize.Width / 2f) / armaSize.Width;

            arma.Scaling = new Vector2(escalaAncho, escalaAncho);
            arma.Position = new Vector2(screenSize.Width - (armaSize.Width * escalaAncho), screenSize.Height - (armaSize.Height * escalaAncho));

            //Cargar modelo de palmera original
            TgcSceneLoader loader1 = new TgcSceneLoader();
            scene = loader1.loadSceneFromFile(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Meshes\\Vegetacion\\Palmera\\Palmera-TgcScene.xml");
            palmeraOriginal = scene.Meshes[0];

            //Cargar Shader personalizado
            effect = TgcShaders.loadEffect(alumnoMediaFolder + "Shaders\\viento.fx");
            palmeraOriginal.Effect = effect;
            palmeraOriginal.Technique = "RenderScene";

            //Crear varias instancias del modelo original, pero sin volver a cargar el modelo entero cada vez
            int rows = 30;
            int cols = 10;
            //float offset = 500;
            arboles = new List<TgcMesh>();
            //bool moverFila=false;

            Random rand = new Random();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    //Crear instancia de modelo
                    TgcMesh instance = palmeraOriginal.createMeshInstance(palmeraOriginal.Name + i + "_" + j);
                    instance.Effect = effect;
                    instance.Technique = "RenderScene";

                    //Desplazarlo
                    instance.move(rand.Next(-10000,10000), 0, rand.Next(-10000,10000));
                    instance.Scale = new Vector3(1.3f, 1.3f, 1.3f);
                    arboles.Add(instance);
                }
            }
            

            //Crear texto 1, b�sico
            vida = new TgcText2d();
            vida.Text = "Vida: 100";
            vida.Color = Color.Red;
            vida.Size = new Size(300, 100);
            vida.changeFont(new System.Drawing.Font("Calibri", 25, FontStyle.Bold));


            //////VARIABLES DE FRUSTUM

            aspectRatio = (float)GuiController.Instance.Panel3d.Width / GuiController.Instance.Panel3d.Height;

            ruedita = 0f;

            //Inicializo angulo de FOV
            anguloFov = FastMath.ToRad(45.0f);

            GuiController.Instance.D3dDevice.Transform.Projection = Matrix.PerspectiveFovLH(anguloFov, aspectRatio, 1f, 50000f);

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

            //Crear un modifier para modificar un v�rtice
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
            /*GuiController.Instance.FpsCamera.Enable = true;
            GuiController.Instance.FpsCamera.setCamera(new Vector3(0,120,0), new Vector3(1, 0, 1));
            //GuiController.Instance.FpsCamera.LookAt(new Vector3(0,120,0));
            GuiController.Instance.FpsCamera.JumpSpeed = 0;
            GuiController.Instance.FpsCamera.MovementSpeed *= 10;*/

            GuiController.Instance.FpsCamera.Enable = false;
            GuiController.Instance.RotCamera.Enable = false;

            camaraQ3 = new Q3FpsCamera();
            camaraQ3.setCamera(posicion, posicion + new Vector3(1.0f, 0.0f, 0.0f));
            camaraQ3.RotationSpeed = velocidadAngular;
            camaraQ3.MovementSpeed = velocidadMov;
            camaraQ3.LockCam = true;

            

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
            
            time += elapsedTime;

            TgcD3dInput input = GuiController.Instance.D3dInput;

            camaraQ3.updateCamera();
            float ang = 0f;

            //float num = (float)GuiController.Instance.Modifiers.getValue("valorFloat");
            if (input.buttonDown(TgcD3dInput.MouseButtons.BUTTON_RIGHT))
            {
                miraActivada = true;
                ruedita -= input.WheelPos;
                ang = 15.0f + ruedita;
                if (ang > 45.0f)
                {
                    ang = 45.0f;
                }
                else if (ang < 2.0f)
                {
                    ang = 2.0f;
                }
                anguloFov = FastMath.ToRad(ang);
                ruedita = ang - 15.0f;
            }
            else
            {
                ruedita = 0f;
                miraActivada = false;
                anguloFov = FastMath.ToRad(45.0f);
            }
            GuiController.Instance.D3dDevice.Transform.Projection = Matrix.PerspectiveFovLH(anguloFov, aspectRatio, 1f, 50000f);

            if (GuiController.Instance.D3dInput.keyDown(Microsoft.DirectX.DirectInput.Key.LeftShift) && !miraActivada)
            {
                camaraQ3.MovementSpeed = velocidadCorrer;
            }
            else
            {
                camaraQ3.MovementSpeed = velocidadMov;
            }

            //Renderizar suelo
            piso.render();
            skyBox.render();

            //Renderizar original e instancias
            original.animateAndRender();
            foreach (TgcSkeletalMesh robot in instanciasMalos)
            {
                robot.animateAndRender();
            }

            // Cargar variables de shader, por ejemplo el tiempo transcurrido.
            effect.SetValue("time", time);

            //Renderizar instancias
            foreach (TgcMesh mesh in arboles)
            {
                mesh.render();
            }

            //DIBUJOS 2D
            //Iniciar dibujado de todos los Sprites de la escena
            GuiController.Instance.Drawer2D.beginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aqu�)
            if (miraActivada)
            {
                mira_zoom.render();
            }
            else
            {
                mira.render();
                arma.render();
            }
            //Finalizar el dibujado de Sprites
            GuiController.Instance.Drawer2D.endDrawSprite();

            vida.render();

            //GuiController.Instance.FpsCamera.setCamera(GuiController.Instance.FpsCamera.Position, GuiController.Instance.FpsCamera.LookAt);
            ///////////////INPUT//////////////////
            //conviene deshabilitar ambas camaras para que no haya interferencia

        }

        /// <summary>
        /// M�todo que se llama cuando termina la ejecuci�n del ejemplo.
        /// Hacer dispose() de todos los objetos creados.
        /// </summary>
        public override void close()
        {
            effect.Dispose();
            original.dispose();
        }

        public void inicializarTerreno()
        {
            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = new Vector3(0, 500, 0);
            skyBox.Size = new Vector3(20000, 20000, 20000);
            string texturesPath = GuiController.Instance.AlumnoEjemplosMediaDir;//GuiController.Instance.ExamplesMediaDir + "Texturas\\Quake\\SkyBox4\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "//FullMoon//up.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "//FullMoon//down.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "//FullMoon//right.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "//FullMoon//left.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "//FullMoon//front.png");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "//FullMoon//back.png");
            skyBox.SkyEpsilon =50f;
            skyBox.updateValues();

            //Crear piso
            TgcTexture pisoTexture = TgcTexture.createTexture(d3dDevice, GuiController.Instance.AlumnoEjemplosMediaDir + "\\pasto2.jpg");
            piso = TgcBox.fromSize(new Vector3(0, 0, 0), new Vector3(20000, 0, 20000), pisoTexture);
            piso.UVTiling = new Vector2(50, 50);
            piso.updateValues();
        }
        

    }
}
