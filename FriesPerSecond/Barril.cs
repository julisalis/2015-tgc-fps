using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.FriesPerSecond
{
    class Barril
    {
        public TgcMesh mesh;
        public bool fueDisparado;
        public Effect efecto;
        public float tiempo = 0f;

        public Barril (TgcMesh mesh)
        {
            this.mesh = mesh;
            this.fueDisparado = false;
        }
    }
}
