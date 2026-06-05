// URL base del backend local.
// Más adelante la cambiaremos por import.meta.env.PUBLIC_API_URL
// para poder desplegar frontend en Vercel y backend en Render.
export const API_URL = "http://localhost:5083";

// Forma estándar de respuesta que usamos en el playground.
// Esto nos ayuda a mostrar statusCode, ok y data siempre igual.
export type ApiResult = {
  statusCode: number;
  ok: boolean;
  data: unknown;
};

// Helper genérico para llamar endpoints que regresan JSON.
// options permite usar GET, POST, PATCH, headers, body, etc.
export async function requestJson(
  url: string,
  options?: RequestInit
): Promise<ApiResult> {
  const response = await fetch(url, options);

  // La mayoría de nuestros endpoints regresan JSON.
  // Si después tenemos endpoints sin body, ajustamos este helper.
  const data: unknown = await response.json();

  return {
    statusCode: response.status,
    ok: response.ok,
    data,
  };
}
