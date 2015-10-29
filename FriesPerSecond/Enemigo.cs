using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;
using TgcViewer.Utils.TgcSkeletalAnimation;

namespace AlumnoEjemplos.FriesPerSecond
{
    public class Enemigo
    {
        public TgcSkeletalMesh meshEnemigo;
        public bool estaVivo;
        public Vector3 ultimaPosicion;
        public Effect efecto;
        public float tiempo = 0f;

        public Enemigo(TgcSkeletalMesh mesh, Vector3 pos)
        {
            this.meshEnemigo = mesh;
            this.estaVivo = true;
            this.ultimaPosicion = pos;
        }
    }
}
