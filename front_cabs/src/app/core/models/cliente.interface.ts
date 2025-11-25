export interface Cliente {
  id: any;
  nombre: any;
  clienteId: number;
  nombreComercial: string | null;
  rfc: string | null;
  activo: boolean;
  legacyClientId: number | null;
  calle: string | null;
  numeroExterior: string | null;
  colonia: string | null;
  codigoPostal: string | null;
  ciudad: string | null;
  estado: string | null;
  pais: string | null;
  telefonoPrincipal: string | null;
  emailPrincipal: string | null;
}
