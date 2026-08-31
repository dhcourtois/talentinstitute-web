export interface EntrevistaPadre {
  id: string;
  nombrePadre: string;
  numeroHijos: number;
  riesgoViolencia: boolean;
  riesgoDivorcio: boolean;
  conoceADios: boolean;
  comentarios: string;
  aceptado: boolean;
  fechaEntrevista: string;
}

export interface RegistrarEntrevistaRequest {
  nombrePadre: string;
  numeroHijos: number;
  riesgoViolencia: boolean;
  riesgoDivorcio: boolean;
  conoceADios: boolean;
  comentarios: string;
  aceptado: boolean;
}

export type NivelAlerta = 'Alta' | 'Media' | 'Sin alertas';

/**
 * Índice de alerta que pide el SOW. Se deriva de los factores de riesgo en el
 * hogar: son la señal que el personal necesita antes de una reunión presencial.
 */
export function nivelAlerta(entrevista: EntrevistaPadre): NivelAlerta {
  if (entrevista.riesgoViolencia) return 'Alta';
  if (entrevista.riesgoDivorcio) return 'Media';
  return 'Sin alertas';
}
