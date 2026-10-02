//#include <stdio.h>
//#include <stdint.h>
//
//
//void TCP_Test_Init(void);
//void Ethernet_Process(void);
//
//void UDP_Test_Init();
//
//void TCP_Init(uint8_t ip1, uint8_t ip2, uint8_t ip3, uint8_t ip4, uint16_t port);
//
//void TCP_Open_Port(uint16_t port);
//
//int tcp_server_send_string(const char *text);
//
//int tcp_server_send(const void *data, uint16_t len);
//
//void TCP_Receive_Callback(uint16_t Port, uint8_t *buffer, uint16_t length);





















#ifndef ETHERNET_H_
#define ETHERNET_H_

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

/*
 * Anzahl gleichzeitig geöffneter lokaler TCP-Server-Ports.
 *
 * Achtung:
 * Diese Werte müssen auch zur lwIP-Konfiguration passen:
 * MEMP_NUM_TCP_PCB_LISTEN >= ETHERNET_MAX_TCP_PORTS
 * MEMP_NUM_TCP_PCB        >= ETHERNET_MAX_TCP_CLIENTS
 */
#define ETHERNET_MAX_TCP_PORTS     8U
#define ETHERNET_MAX_TCP_CLIENTS   8U


/* Ethernet/lwIP einmalig initialisieren */
int Ethernet_Init(uint8_t ip1,
                  uint8_t ip2,
                  uint8_t ip3,
                  uint8_t ip4);

/*
 * Kompatibilitätsfunktion:
 * Initialisiert Ethernet und öffnet direkt den ersten TCP-Port.
 */
int TCP_Init(uint8_t ip1,
             uint8_t ip2,
             uint8_t ip3,
             uint8_t ip4,
             uint16_t first_port);


/* TCP-Server-Port öffnen bzw. schließen */
int TCP_Open_Port(uint16_t port);
int TCP_Close_Port(uint16_t port);

int TCP_Port_Is_Open(uint16_t port);
uint16_t TCP_Get_Client_Count(uint16_t port);


/*
 * Sendet an alle TCP-Clients, die über diesen lokalen Server-Port
 * verbunden sind.
 *
 * Rückgabe:
 *   > 0  Anzahl erfolgreich bedienter Verbindungen
 *   -1   kein Client auf diesem Port verbunden
 *   -2   ungültige Daten
 *   < 0  Fehler
 */
int TCP_Send_To_Port(uint16_t local_port,
                     const void *data,
                     uint16_t len);

int TCP_Send_String_To_Port(uint16_t local_port,
                            const char *text);


/*
 * Alte Funktionsnamen für bestehenden Code.
 * Diese senden weiterhin über den Default-Port 5001.
 */
int tcp_server_send(const void *data, uint16_t len);
int tcp_server_send_string(const char *text);


/* Muss regelmäßig in deiner while(1)-Schleife aufgerufen werden. */
void Ethernet_Process(void);


/* Beispiel-Initialisierungen */
void TCP_Test_Init(void);
void UDP_Test_Init(void);


/* UDP-Funktion aus deiner bisherigen Klasse */
int udp_server_send_to_ip(const void *data,
                          uint16_t len,
                          uint8_t ip0,
                          uint8_t ip1,
                          uint8_t ip2,
                          uint8_t ip3,
                          uint16_t port);


/*
 * Diese Funktionen kannst du in einer anderen .c-Datei definieren.
 * Die schwachen Standardimplementierungen in Ethernet.c werden dann ersetzt.
 *
 * buffer bleibt nur während dieses Callbacks gültig.
 * Daten bei Bedarf in einen eigenen Puffer kopieren.
 */
void TCP_Receive_Callback(uint16_t local_port,
                          uint8_t *buffer,
                          uint16_t length);

void TCP_Connected_Callback(uint16_t local_port);

void TCP_Disconnected_Callback(uint16_t local_port);

#ifdef __cplusplus
}
#endif

#endif /* ETHERNET_H_ */
