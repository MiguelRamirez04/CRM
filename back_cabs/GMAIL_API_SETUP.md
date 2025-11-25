# Configuración de Gmail API para envío de correos

## Paso 1: Crear proyecto en Google Cloud Console

1. Ve a https://console.cloud.google.com/
2. Crea un nuevo proyecto o selecciona uno existente
3. Habilita la API de Gmail:
   - Ve a "APIs y servicios" > "Biblioteca"
   - Busca "Gmail API"
   - Haz clic en "Habilitar"

## Paso 2: Crear credenciales OAuth 2.0

1. Ve a "APIs y servicios" > "Credenciales"
2. Haz clic en "Crear credenciales" > "ID de cliente de OAuth"
3. Selecciona "Aplicación web"
4. Configura:
   - Nombre: "CRM Cabs API"
   - URIs de redireccionamiento autorizados: `http://localhost` (para desarrollo)
5. Guarda el Client ID y Client Secret

## Paso 3: Obtener Refresh Token

Para obtener el refresh token, necesitas autorizar la aplicación una vez:

1. Instala Google OAuth2 tools (opcional):
   ```bash
   dotnet tool install -g Google.Apis.Auth
   ```

2. Crea un script simple para obtener el refresh token (guarda como `get_refresh_token.cs`):
   ```csharp
   using Google.Apis.Auth.OAuth2;
   using System;
   using System.Threading;

   class Program
   {
       static void Main(string[] args)
       {
           var clientId = "TU_CLIENT_ID";
           var clientSecret = "TU_CLIENT_SECRET";

           var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
               new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
               new[] { "https://www.googleapis.com/auth/gmail.send" },
               "user",
               CancellationToken.None).Result;

           Console.WriteLine($"Refresh Token: {credential.Token.RefreshToken}");
       }
   }
   ```

3. Ejecuta el script y autoriza la aplicación en el navegador
4. Copia el refresh token que se muestra

## Paso 4: Configurar en appsettings.json

Actualiza la sección `GmailSettings` en `appsettings.json`:

```json
"GmailSettings": {
  "ClientId": "tu_client_id_aqui",
  "ClientSecret": "tu_client_secret_aqui",
  "RefreshToken": "tu_refresh_token_aqui",
  "SenderEmail": "tu_correo@gmail.com",
  "UseGmailApi": true
}
```

## Paso 5: Probar

1. Reinicia la aplicación
2. Prueba el endpoint de recuperación de contraseña
3. Verifica que se envíe el email a través de Gmail API

## Notas importantes

- El refresh token no expira automáticamente, pero puede invalidarse si cambias la contraseña o revocan permisos
- Para producción, considera usar una cuenta de servicio en lugar de OAuth 2.0
- Asegúrate de que la cuenta de Gmail tenga habilitada la verificación en 2 pasos para mayor seguridad

## Solución de problemas

- Si obtienes errores de autenticación, verifica que las credenciales sean correctas
- Si el refresh token no funciona, genera uno nuevo siguiendo el Paso 3
- Para desarrollo local, puedes mantener `UseGmailApi: false` para usar el modo test