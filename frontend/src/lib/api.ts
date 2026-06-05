// URL local de la API.
// Más adelante la moveremos a una variable de entorno para producción.
export const apiUrl = "http://localhost:5083";

export type ApiResult = {
  statusCode: number;
  ok: boolean;
  data: unknown;
};

// API FETCH WRAPPER (requestJson):
// Abstracción reutilizable del fetch nativo.
// Simplifica las llamadas asíncronas encapsulando la extracción del body asíncrono y mapeando
// cabeceras y métodos dinámicos (GET, POST, PATCH), centralizando la obtención del código de estado HTTP.
export async function requestJson(
  url: string,
  options?: RequestInit
): Promise<ApiResult> {
  const response = await fetch(url, options);
  const data: unknown = await response.json();

  return {
    statusCode: response.status,
    ok: response.ok,
    data,
  };
}
