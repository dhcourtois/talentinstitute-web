using System;

namespace TalentInstitute.Domain.Entities;

public class Pace
{
    public Guid Id { get; private set; }
    public string Materia { get; private set; } // e.g. MAT
    public int Numero { get; private set; } // e.g. 1097
    public int PuntajeMaximo { get; private set; }
    public int PuntajeMinimoAprobacion { get; private set; }

    public Pace(string materia, int numero, int puntajeMaximo = 100, int puntajeMinimoAprobacion = 80)
    {
        Id = Guid.NewGuid();
        Materia = materia;
        Numero = numero;
        PuntajeMaximo = puntajeMaximo;
        PuntajeMinimoAprobacion = puntajeMinimoAprobacion;
    }
}
