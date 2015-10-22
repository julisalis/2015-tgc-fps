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

        public Enemigo(TgcSkeletalMesh mesh)
        {
            this.meshEnemigo = mesh;
            this.estaVivo = true;
        }
    }
}
