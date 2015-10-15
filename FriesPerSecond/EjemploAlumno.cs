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
        Vector2 posicionArmaDisparo;
        Vector2 posicionArmaOriginal;

        Effect effect;
        float time;

        //Sonidos
        TgcStaticSound disparo;

        protected Point mouseCenter;

        //Bounding
        TgcBoundingBox boundingCamara;
        Vector3 boundingCamScale;
        Vector3 ultimaPosCamara;

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
        float velocidadEnemigos = -5f;

        //Musica
        //TgcMp3Player musicaFondo = GuiController.Instance.Mp3Player;
        string pathMusica;

        //Camara
        Q3FpsCamera camaraQ3;

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

            pathMusica = GuiController.Instance.AlumnoEjemplosMediaDir + "Sonidos\\Stayin_alive.mp3";
            GuiController.Instance.Mp3Player.closeFile();
            GuiController.Instance.Mp3Player.FileName = pathMusica;

            TgcMp3Player player = GuiController.Instance.Mp3Player;

            player.play(true);

            //Cargar malla original
            TgcSkeletalLoader loader = new TgcSkeletalLoader();
            string pathMesh = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\CS_Arctic-TgcSkeletalMesh.xml";
            string mediaPath = GuiController.Instance.ExamplesMediaDir + "SkeletalAnimations\\BasicHuman\\";
            original = loader.loadMeshFromFile(pathMesh, mediaPath);
            original.Scale = new Vector3(3.4f, 3.4f,3.4f);
            
            //Agregar animación a original
            loader.loadAnimationFromFile(original, mediaPath + "\\Animations\\Walk-TgcSkeletalAnim.xml");

            
            
            //Crear Sprite
            mira = new TgcSprite();
            mira.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\mira.png");
            miraActivada = false;
            mira_zoom = new TgcSprite();
            mira_zoom.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\mira_zoom3.png");

            arma = new TgcSprite();
            arma.Texture = TgcTexture.createTexture(alumnoMediaFolder + "\\arma.png");

            //Sonidos
            disparo = new TgcStaticSound();
            disparo.loadSound(GuiController.Instance.ExamplesMediaDir + "\\Sound\\explosión, pequeña.wav");

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
            
            posicionArmaDisparo = new Vector2(arma.Position.X + 5f, arma.Position.Y + 5f);
            posicionArmaOriginal = arma.Position;

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
            

            //Crear texto 1, básico
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

            //Crear un modifier para modificar un vértice
            GuiController.Instance.Modifiers.addVertex3f("valorVertice", new Vector3(-100, -100, -100), new Vector3(50, 50, 50), new Vector3(0, 0, 0));



            ///////////////CONFIGURAR CAMARA ROTACIONAL//////////////////
            //Es la camara que viene por default, asi que no hace falta hacerlo siempre
            //GuiController.Instance.RotCamera.Enable = true;
            //Configurar centro al que se mira y distancia desde la que se mira
            //GuiController.Instance.RotCamera.setCamera(new Vector3(0, 0, 0), 300);
            Vector3 posicion = new Vector3();
            posicion = original.Position;
            posicion.Y += 150;
            posicion.Z += 0;
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

            ultimaPosCamara = camaraQ3.getPosition();
            Vector3 posBound = new Vector3(camaraQ3.getPosition().X, camaraQ3.getPosition().Y + 30, camaraQ3.getPosition().Z);
            boundingCamara = new TgcBoundingBox();
            boundingCamScale = new Vector3(1f, 1f, 1f);
            boundingCamara.scaleTranslate(posBound, boundingCamScale);

            //ENEMIGOS          
            instanciasMalos = new List<TgcSkeletalMesh>();
            //El ultimo parametro es el radio
            crearPersonajes(4, 3, original, instanciasMalos, 3.4f, 100.0f);

            
            
        }


        
        public override void render(float elapsedTime)
        {
            //Device de DirectX para renderizar
            d3dDevice = GuiController.Instance.D3dDevice;
            
            time += elapsedTime;

            TgcD3dInput input = GuiController.Instance.D3dInput;

            camaraQ3.updateCamera();
            
            /*
            //boundingCamara.scaleTranslate(camaraQ3.getPosition(), boundingCamScale);
            boundingCamara.setRenderColor(Color.Red);
            boundingCamara.render();
            */
             
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

            //Renderizar original e instancias (no dibujo original, solo instancias)
            //original.animateAndRender();
            foreach (TgcSkeletalMesh enemigo in instanciasMalos)
            {
                enemigo.animateAndRender();
                rotarMesh(enemigo);
                enemigo.moveOrientedY(velocidadEnemigos);
                foreach (TgcMesh arbol in arboles)
                {
                    if (TgcCollisionUtils.testAABBAABB(enemigo.BoundingBox,arbol.BoundingBox))
                    {
                        enemigo.moveOrientedY(-velocidadEnemigos);
                    }
                }
                
            }

            // Cargar variables de shader, por ejemplo el tiempo transcurrido.
            effect.SetValue("time", time);

            //Renderizar instancias
            foreach (TgcMesh mesh in arboles)
            {
                mesh.render();
                mesh.BoundingBox.render();
                
            }
            ultimaPosCamara = camaraQ3.getPosition();

            //DIBUJOS 2D
            //Iniciar dibujado de todos los Sprites de la escena
            GuiController.Instance.Drawer2D.beginDrawSprite();

            //Dibujar sprite (si hubiese mas, deberian ir todos aquí)
            if (miraActivada)
            {
                mira_zoom.render();
            }
            else
            {
                mira.render();
                arma.render();
            }
            //DISPARO
            if (input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT) && !miraActivada)
            {                
                arma.Position = posicionArmaDisparo;
                arma.render();
                disparo.play(false);
                arma.Position = posicionArmaOriginal;
            }
            else if (input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT) && miraActivada)
            {
                disparo.play(false);
            }
            //Finalizar el dibujado de Sprites
            GuiController.Instance.Drawer2D.endDrawSprite();

            vida.render();

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

        public void crearPersonajes(int columnas,int filas,TgcSkeletalMesh meshOriginal, List<TgcSkeletalMesh> lista,float scale,float radio)
        {
            Random rand1 = new Random();
            float x=0f;
            float z=0f;
            //instanciasMalos = new List<TgcSkeletalMesh>();
            //int rowsRobot = 3;
            //int colsRobot = 3;
            for (int k = 0; k < filas; k++)
            {
                for (int q = 0; q < columnas; q++)
                {
                    //Crear instancia de modelo
                    TgcSkeletalMesh instance = meshOriginal.createMeshInstance(original.Name + k + "_" + q);
                    
                    do
                    {
                        x = rand1.Next(-5000, 5000);
                        z = rand1.Next(-5000, 5000);
                    } while (FastMath.Sqrt(x*x+z*z)<=radio);
                    //Achico el bounding box del arbol
                    //instance.BoundingBox.scaleTranslate(new Vector3(0f, 0f, 0f), new Vector3(0.5f, 0.5f, 0.5f));
                    

                    //instance.rotateY(FastMath.Atan(3f));
                    //Desplazarlo
                    instance.move(x, 0, z);

                    //Roto el mesh
                    rotarMesh(instance);

                    instance.Scale = new Vector3(scale, scale, scale);
                    lista.Add(instance);
                }
            }

            original.playAnimation("Walk");
            foreach (TgcSkeletalMesh robot in lista)
            {
                robot.playAnimation("Walk");
            }
            
        } 

        public void rotarMesh(TgcSkeletalMesh mesh1)
        {
            Vector3 haciaDondeDebeMirar;
            haciaDondeDebeMirar = mesh1.Position - camaraQ3.getPosition();
            haciaDondeDebeMirar.Y=0;
            haciaDondeDebeMirar.Normalize();
            mesh1.rotateY((float)FastMath.Atan2(haciaDondeDebeMirar.X, haciaDondeDebeMirar.Z) - mesh1.Rotation.Y) ;
        }
        

    }
}
