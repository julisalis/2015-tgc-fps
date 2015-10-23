using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TgcViewer.Utils.TgcGeometry;
using TgcViewer.Utils.TgcSceneLoader;

namespace AlumnoEjemplos.FriesPerSecond
{
    class Bala
    {
        //public TgcBox box;
        public Vector3 velocidadVectorial;
        public TgcRay ray;

        public Bala()
        {
            velocidadVectorial = new Vector3(0f,0f,0f);
        }
    }
}
