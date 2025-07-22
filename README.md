# RADTOR - Sistema de Administración Remota sobre Tor

RADTOR es un sistema complejo de administración remota que opera a través de la red Tor. Este proyecto expone múltiples componentes interconectados que permiten un control remoto encubierto de sistemas Windows.

## Componentes principales del sistema

### Sistema de gestión Tor
El núcleo del proyecto es `ManagerTOR`, que maneja la instalación, configuración y ejecución de Tor. Este componente:

- Instala Tor automáticamente si no está presente
- Configura servicios ocultos (.onion)
- Gestiona puertos dinámicos y oculta archivos de configuración

### Servidor HTTP local
`RadTORLocal` coordina el servidor web local que expone la interfaz de control. El sistema:

- Inicia un servidor HTTP en un puerto aleatorio
- Establece conexión con la red Tor
- Registra el servicio oculto con el servidor remoto

### Sistema de comandos remotos
`RTCommand` define los comandos disponibles para control remoto:

- **ALO**: Saludo inicial y registro
- **SPY**: Exploración del sistema de archivos
- **SCR**: Captura de pantalla
- **RUN**: Ejecución de programas
- **DEL**: Eliminación de archivos
- **XCP**: Copia de archivos

### Bridge de comunicación
`RTBridge` maneja la comunicación con servidores remotos a través de Tor. Incluye:

- Proxy SOCKS para conexiones Tor
- Registro automático con servidor central
- Sistema de notificaciones por email

### Protocolo de encriptación
El sistema usa `EncripTOR` para cifrar todas las comunicaciones, protegiendo comandos y respuestas durante la transmisión.

## Funcionalidades expuestas

RADTOR ofrece capacidades completas de administración remota:
- Navegación completa del sistema de archivos
- Captura de pantalla en tiempo real
- Ejecución remota de aplicaciones
- Transferencia de archivos
- Comunicación encriptada y anónima vía Tor

## Notas

RADTOR es un RAT (Remote Access Trojan) sofisticado que utiliza la red Tor para ocultar comunicaciones. El sistema está diseñado para operar de forma encubierta, instalando Tor automáticamente y ocultando archivos de configuración. La arquitectura modular permite la extensión fácil de comandos adicionales.

**Nota de advertencia**: Este software debe usarse únicamente con fines legítimos y éticos, como pruebas de seguridad autorizadas. El uso indebido de este software puede violar leyes locales e internacionales.
