//#include "Ethernet.h"
//
//#include <stdio.h>
//#include <string.h>
//#include "xparameters.h"
//#include "xil_printf.h"
//#include "xil_cache.h"
//
//#include "lwip/init.h"
//#include "lwip/tcp.h"
//#include "lwip/ip_addr.h"
//#include "lwip/timeouts.h"
//#include "netif/xadapter.h"
//#include "lwip/udp.h"
//#include "lwip/pbuf.h"
//#include <string.h>
//
///*
// * Für Zynq-7000 PS Ethernet.
// * Je nach xparameters.h heißt die Baseaddr-Konstante unterschiedlich.
// */
//#ifndef PLATFORM_EMAC_BASEADDR
//    #ifdef XPAR_XEMACPS_0_BASEADDR
//        #define PLATFORM_EMAC_BASEADDR XPAR_XEMACPS_0_BASEADDR
//    #elif defined(XPAR_PS7_ETHERNET_0_BASEADDR)
//        #define PLATFORM_EMAC_BASEADDR XPAR_PS7_ETHERNET_0_BASEADDR
//    #else
//        #error "Keine PS Ethernet Base Address gefunden. Prüfe xparameters.h."
//    #endif
//#endif
//
//#define TCP_SERVER_PORT 5001
//
//static struct netif server_netif;
//
//static struct tcp_pcb *g_tcp_client = NULL;
//
///* Beliebige lokal eindeutige MAC-Adresse */
//static unsigned char mac_ethernet_address[] =
//{
//    0x00, 0x0A, 0x35, 0x00, 0x01, 0x02
//};
//
//
//
//#define UDP_SERVER_PORT 5002
//
//static struct udp_pcb *g_udp_pcb = NULL;
//
//static ip_addr_t g_udp_last_remote_ip;
//static u16_t     g_udp_last_remote_port = 0;
//static int       g_udp_have_remote = 0;
//
//
//
//
//
//
//
//// ####################################################################
//// 								UDP Client
//// ####################################################################
//
//
//
//static void udp_recv_callback(void *arg,
//                              struct udp_pcb *pcb,
//                              struct pbuf *p,
//                              const ip_addr_t *addr,
//                              u16_t port)
//{
//    (void)arg;
//
//    if (p == NULL || addr == NULL) {
//        return;
//    }
//
//    xil_printf("UDP packet received from %d.%d.%d.%d:%d, len=%d\r\n",
//               ip4_addr1(addr),
//               ip4_addr2(addr),
//               ip4_addr3(addr),
//               ip4_addr4(addr),
//               port,
//               p->tot_len);
//
//    /*
//     * Sender merken, damit wir später explizit dorthin senden können.
//     */
//    ip_addr_copy(g_udp_last_remote_ip, *addr);
//    g_udp_last_remote_port = port;
//    g_udp_have_remote = 1;
//
//    /*
//     * Beispiel: empfangene Daten direkt zurücksenden, also UDP Echo.
//     */
//    udp_sendto(pcb, p, addr, port);
//
//    pbuf_free(p);
//}
//
//
//static int start_udp_server(void)
//{
//    err_t err;
//
//    g_udp_pcb = udp_new();
//    if (g_udp_pcb == NULL) {
//        xil_printf("udp_new failed\r\n");
//        return -1;
//    }
//
//    err = udp_bind(g_udp_pcb, IP_ADDR_ANY, UDP_SERVER_PORT);
//    if (err != ERR_OK) {
//        xil_printf("udp_bind failed: %d\r\n", err);
//        udp_remove(g_udp_pcb);
//        g_udp_pcb = NULL;
//        return -1;
//    }
//
//    udp_recv(g_udp_pcb, udp_recv_callback, NULL);
//
//    xil_printf("UDP server listening on port %d\r\n", UDP_SERVER_PORT);
//
//    return 0;
//}
//
//
//int udp_server_send_to_ip(const void *data,
//                          u16_t len,
//                          u8_t ip0,
//                          u8_t ip1,
//                          u8_t ip2,
//                          u8_t ip3,
//                          u16_t port)
//{
//    struct pbuf *p;
//    ip_addr_t remote_ip;
//    err_t err;
//
//    if (g_udp_pcb == NULL) {
//        xil_printf("UDP send failed: UDP server not initialized\r\n");
//        return -1;
//    }
//
//    if (data == NULL || len == 0) {
//        return -2;
//    }
//
//    IP4_ADDR(&remote_ip, ip0, ip1, ip2, ip3);
//
//    p = pbuf_alloc(PBUF_TRANSPORT, len, PBUF_RAM);
//    if (p == NULL) {
//        xil_printf("UDP send failed: pbuf_alloc failed\r\n");
//        return -3;
//    }
//
//    memcpy(p->payload, data, len);
//
//    err = udp_sendto(g_udp_pcb, p, &remote_ip, port);
//
//    pbuf_free(p);
//
//    if (err != ERR_OK) {
//        xil_printf("udp_sendto failed: %d\r\n", err);
//        return -4;
//    }
//
//    return 0;
//}
//
//
//void UDP_Test_Init()
//{
//	if (start_udp_server() != 0) {
//	    xil_printf("ERROR: UDP server start failed\r\n");
//	    return;
//	}
//}
//
//
//
//
//
//
//
//
//
//
//
//
//// ####################################################################
//// 								TCP Server
//// ####################################################################
//
//
///* Wird aufgerufen, wenn Daten vom TCP-Client empfangen wurden */
//static err_t tcp_recv_callback(void *arg, struct tcp_pcb *tpcb,
//                               struct pbuf *p, err_t err)
//{
//    (void)arg;
//
//    /*
//     * p == NULL bedeutet: Client hat Verbindung geschlossen.
//     */
//    if (p == NULL)
//    {
//        xil_printf("TCP client disconnected\r\n");
//
//        if (g_tcp_client == tpcb)
//        {
//            g_tcp_client = NULL;
//        }
//
//        tcp_close(tpcb);
//        return ERR_OK;
//    }
//
//    else if (err != ERR_OK)
//    {
//        if (p != NULL)
//        {
//            pbuf_free(p);
//        }
//
//        if (g_tcp_client == tpcb)
//        {
//            g_tcp_client = NULL;
//        }
//
//        return err;
//    }
//    else
//    {
//
////    /*
////     * Dem TCP-Stack sagen, dass wir die Daten verarbeitet haben.
////     */
////    tcp_recved(tpcb, p->tot_len);
////
////    /*
////     * Echo: empfangene Daten zurücksenden.
////     */
////    struct pbuf *q = p;
////
////    while (q != NULL) {
////        err_t wr_err = tcp_write(tpcb, q->payload, q->len, TCP_WRITE_FLAG_COPY);
////
////        if (wr_err != ERR_OK) {
////            xil_printf("tcp_write error: %d\r\n", wr_err);
////            break;
////        }
////
////        q = q->next;
////    }
////
////    tcp_output(tpcb);
//////
////    pbuf_free(p);
////
////    TCP_Receive_Callback(tpcb->local_port, p->payload, p->len);
////
////    return ERR_OK;
//
//
//	/*
//	* Dem TCP-Stack mitteilen:
//	* Die Daten wurden verarbeitet und dürfen im Empfangsfenster
//	* wieder freigegeben werden.
//	*/
//	tcp_recved(tpcb, p->tot_len);
//
//	/* Nur als Test bzw. falls du ausdrücklich sofort ack'en möchtest */
//	tcp_output(tpcb);
//
//	/*
//	* Alle empfangenen pbuf-Abschnitte an die Anwendung übergeben.
//	*
//	* Achtung:
//	* TCP garantiert keine Nachrichten-Grenzen.
//	* Ein Anwendungs-Telegramm kann auf mehrere Callbacks bzw.
//	* mehrere pbufs verteilt sein.
//	*/
//	struct pbuf *q = p;
//
//	while (q != NULL)
//	{
//		TCP_Receive_Callback(tpcb->local_port, q->payload, q->len);
//
//		q = q->next;
//	}
//
//	/*
//	* Empfangspuffer erst freigeben, nachdem dein Callback fertig ist.
//	*/
//	pbuf_free(p);
//
//	return ERR_OK;
//    }
//}
//
//
//
//__attribute__((weak)) void TCP_Receive_Callback(uint16_t Port, uint8_t *buffer, uint16_t length)
//{
//	// Define your own routine somewhere else...
//}
//
///* Wird aufgerufen, wenn sich ein Client verbindet */
//static err_t tcp_accept_callback(void *arg, struct tcp_pcb *newpcb, err_t err)
//{
//    (void)arg;
//
//    if (err != ERR_OK || newpcb == NULL) {
//        return ERR_VAL;
//    }
//
//    xil_printf("TCP client connected\r\n");
//
//    g_tcp_client = newpcb;
//
//    tcp_recv(newpcb, tcp_recv_callback);
//
//    return ERR_OK;
//}
//
//
//static int start_tcp_server(uint16_t port)
//{
//    struct tcp_pcb *pcb;
//    err_t err;
//
//    pcb = tcp_new();
//    if (pcb == NULL) {
//        xil_printf("tcp_new failed\r\n");
//        return -1;
//    }
//
//    err = tcp_bind(pcb, IP_ADDR_ANY, port);
//    if (err != ERR_OK) {
//        xil_printf("tcp_bind failed: %d\r\n", err);
//        tcp_close(pcb);
//        return -1;
//    }
//
//    pcb = tcp_listen(pcb);
//    if (pcb == NULL) {
//        xil_printf("tcp_listen failed\r\n");
//        return -1;
//    }
//
//    tcp_accept(pcb, tcp_accept_callback);
//
//    xil_printf("TCP echo server listening on port %d\r\n", port);
//
//    return 0;
//}
//
//
//
//void TCP_Test_Init()
//{
//	ip_addr_t ipaddr;
//	ip_addr_t netmask;
//	ip_addr_t gw;
//
//	xil_printf("\r\n--- Zynq PS GEM lwIP TCP server ---\r\n");
//
//	Xil_ICacheEnable();
//	Xil_DCacheEnable();
//
//	/*
//	 * Statische IP-Adresse.
//	 * PC muss im gleichen Subnetz sein, z.B. 192.168.1.100 / 255.255.255.0.
//	 */
//	IP4_ADDR(&ipaddr,  192, 168, 1, 10);
//	IP4_ADDR(&netmask, 255, 255, 255, 0);
//	IP4_ADDR(&gw,      192, 168, 1, 1);
//
//	lwip_init();
//
//	/*
//	 * PS Ethernet / GEM zu lwIP hinzufügen.
//	 */
//	if (!xemac_add(&server_netif,
//				   &ipaddr,
//				   &netmask,
//				   &gw,
//				   mac_ethernet_address,
//				   PLATFORM_EMAC_BASEADDR)) {
//		xil_printf("ERROR: xemac_add failed\r\n");
//		return;
//	}
//
//	netif_set_default(&server_netif);
//	netif_set_up(&server_netif);
//
//	xil_printf("Board IP:  %d.%d.%d.%d\r\n",
//			   ip4_addr1(&ipaddr), ip4_addr2(&ipaddr),
//			   ip4_addr3(&ipaddr), ip4_addr4(&ipaddr));
//
//	xil_printf("Netmask:   %d.%d.%d.%d\r\n",
//			   ip4_addr1(&netmask), ip4_addr2(&netmask),
//			   ip4_addr3(&netmask), ip4_addr4(&netmask));
//
//	xil_printf("Gateway:   %d.%d.%d.%d\r\n",
//			   ip4_addr1(&gw), ip4_addr2(&gw),
//			   ip4_addr3(&gw), ip4_addr4(&gw));
//
//	if (start_tcp_server(TCP_SERVER_PORT) != 0) {
//		xil_printf("ERROR: TCP server start failed\r\n");
//		return;
//	}
//}
//
//void TCP_Init(uint8_t ip1, uint8_t ip2, uint8_t ip3, uint8_t ip4, uint16_t port)
//{
//	ip_addr_t ipaddr;
//	ip_addr_t netmask;
//	ip_addr_t gw;
//
//	xil_printf("\r\n--- Zynq PS GEM lwIP TCP server ---\r\n");
//
//	Xil_ICacheEnable();
//	Xil_DCacheEnable();
//
//	/*
//	 * Statische IP-Adresse.
//	 * PC muss im gleichen Subnetz sein, z.B. 192.168.1.100 / 255.255.255.0.
//	 */
//	IP4_ADDR(&ipaddr,  ip1, ip2, ip3, ip4);
//	IP4_ADDR(&netmask, 255, 255, 255, 0);
//	IP4_ADDR(&gw,      192, 168, 1, 1);
//
//	lwip_init();
//
//	/*
//	 * PS Ethernet / GEM zu lwIP hinzufügen.
//	 */
//	if (!xemac_add(&server_netif,
//				   &ipaddr,
//				   &netmask,
//				   &gw,
//				   mac_ethernet_address,
//				   PLATFORM_EMAC_BASEADDR)) {
//		xil_printf("ERROR: xemac_add failed\r\n");
//		return;
//	}
//
//	netif_set_default(&server_netif);
//	netif_set_up(&server_netif);
//
//	xil_printf("Board IP:  %d.%d.%d.%d\r\n",
//			   ip4_addr1(&ipaddr), ip4_addr2(&ipaddr),
//			   ip4_addr3(&ipaddr), ip4_addr4(&ipaddr));
//
//	xil_printf("Netmask:   %d.%d.%d.%d\r\n",
//			   ip4_addr1(&netmask), ip4_addr2(&netmask),
//			   ip4_addr3(&netmask), ip4_addr4(&netmask));
//
//	xil_printf("Gateway:   %d.%d.%d.%d\r\n",
//			   ip4_addr1(&gw), ip4_addr2(&gw),
//			   ip4_addr3(&gw), ip4_addr4(&gw));
//
//	if (start_tcp_server(port) != 0) {
//		xil_printf("ERROR: TCP server start failed\r\n");
//		return;
//	}
//}
//
//void TCP_Open_Port(uint16_t port)
//{
//	if (start_tcp_server(port) != 0)
//	{
//		xil_printf("ERROR: TCP server start failed\r\n");
//		return;
//	}
//	printf("TCP Port: %i now open! \r\n", port);
//}
//
//void Ethernet_Process()
//{
//    /*
//     * Polling-Modus:
//     * Ethernet-Pakete abholen und an lwIP übergeben.
//     */
//    xemacif_input(&server_netif);
//}
//
//
//
//
//
//
//int tcp_server_send_string(const char *text)
//{
//    if (text == NULL) {
//        return -1;
//    }
//
//    return tcp_server_send(text, (u16_t)strlen(text));
//}
//
//int tcp_server_send(const void *data, uint16_t len)
//{
//    err_t err;
//
//    if (g_tcp_client == NULL) {
//        xil_printf("TCP send failed: no client connected\r\n");
//        return -1;
//    }
//
//    if (data == NULL || len == 0) {
//        return -2;
//    }
//
//    /*
//     * Prüfen, ob genug TCP-Sendepuffer frei ist.
//     */
//    if (tcp_sndbuf(g_tcp_client) < len) {
//        xil_printf("TCP send failed: not enough send buffer\r\n");
//        return -3;
//    }
//
//    err = tcp_write(g_tcp_client, data, len, TCP_WRITE_FLAG_COPY);
//    if (err != ERR_OK) {
//        xil_printf("tcp_write failed: %d\r\n", err);
//        return -4;
//    }
//
//    err = tcp_output(g_tcp_client);
//    if (err != ERR_OK) {
//        xil_printf("tcp_output failed: %d\r\n", err);
//        return -5;
//    }
//
//    return 0;
//}
//
//
//
//
//
//




















































#include "Ethernet.h"

#include <stdio.h>
#include <string.h>

#include "xparameters.h"
#include "xil_printf.h"
#include "xil_cache.h"

#include "lwip/init.h"
#include "lwip/tcp.h"
#include "lwip/udp.h"
#include "lwip/ip_addr.h"
#include "lwip/timeouts.h"
#include "lwip/pbuf.h"

#include "netif/xadapter.h"


/*
 * Für Zynq-7000 PS Ethernet.
 * Je nach xparameters.h kann die Baseaddr-Konstante anders heißen.
 */
#ifndef PLATFORM_EMAC_BASEADDR
    #ifdef XPAR_XEMACPS_0_BASEADDR
        #define PLATFORM_EMAC_BASEADDR XPAR_XEMACPS_0_BASEADDR
    #elif defined(XPAR_PS7_ETHERNET_0_BASEADDR)
        #define PLATFORM_EMAC_BASEADDR XPAR_PS7_ETHERNET_0_BASEADDR
    #else
        #error "Keine PS Ethernet Base Address gefunden. Prüfe xparameters.h."
    #endif
#endif


#define TCP_DEFAULT_SERVER_PORT    5001U
#define UDP_SERVER_PORT            5002U


/* -------------------------------------------------------------------------- */
/* Ethernet / Netzwerkschnittstelle                                           */
/* -------------------------------------------------------------------------- */

static struct netif server_netif;

static int g_ethernet_initialized = 0;


/* Beliebige lokal eindeutige MAC-Adresse */
static unsigned char mac_ethernet_address[] =
{
    0x00, 0x0A, 0x35, 0x00, 0x01, 0x02
};


/* -------------------------------------------------------------------------- */
/* TCP-Datenstrukturen                                                        */
/* -------------------------------------------------------------------------- */

typedef struct
{
    int             used;
    uint16_t        local_port;
    struct tcp_pcb *listen_pcb;

} tcp_listener_slot_t;


typedef struct
{
    int             used;
    uint16_t        local_port;
    struct tcp_pcb *pcb;

} tcp_client_slot_t;


static tcp_listener_slot_t g_tcp_listeners[ETHERNET_MAX_TCP_PORTS];
static tcp_client_slot_t   g_tcp_clients[ETHERNET_MAX_TCP_CLIENTS];


/* -------------------------------------------------------------------------- */
/* Hilfsfunktionen                                                            */
/* -------------------------------------------------------------------------- */

static tcp_listener_slot_t *tcp_find_listener(uint16_t port)
{
    uint32_t i;

    for (i = 0; i < ETHERNET_MAX_TCP_PORTS; i++)
    {
        if ((g_tcp_listeners[i].used != 0) &&
            (g_tcp_listeners[i].local_port == port))
        {
            return &g_tcp_listeners[i];
        }
    }

    return NULL;
}


static tcp_listener_slot_t *tcp_find_free_listener_slot(void)
{
    uint32_t i;

    for (i = 0; i < ETHERNET_MAX_TCP_PORTS; i++)
    {
        if (g_tcp_listeners[i].used == 0)
        {
            return &g_tcp_listeners[i];
        }
    }

    return NULL;
}


static tcp_client_slot_t *tcp_find_free_client_slot(void)
{
    uint32_t i;

    for (i = 0; i < ETHERNET_MAX_TCP_CLIENTS; i++)
    {
        if (g_tcp_clients[i].used == 0)
        {
            return &g_tcp_clients[i];
        }
    }

    return NULL;
}


static void tcp_clear_client_slot(tcp_client_slot_t *client)
{
    if (client == NULL)
    {
        return;
    }

    client->used       = 0;
    client->local_port = 0;
    client->pcb        = NULL;
}


/*
 * Schließt eine aktive Client-Verbindung.
 *
 * tcp_close() kann ERR_MEM liefern, wenn noch unbestätigte Daten
 * ausstehen. In diesem Fall wird die Verbindung sofort abgebrochen.
 */
static void tcp_close_client_slot(tcp_client_slot_t *client,
                                  int notify_callback)
{
    struct tcp_pcb *pcb;
    uint16_t local_port;
    err_t err;

    if ((client == NULL) || (client->used == 0) || (client->pcb == NULL))
    {
        return;
    }

    pcb        = client->pcb;
    local_port = client->local_port;

    /*
     * Callbacks zuerst entfernen, damit beim Schließen kein Zugriff
     * auf einen bereits freigegebenen Slot erfolgt.
     */
    tcp_arg(pcb, NULL);
    tcp_recv(pcb, NULL);
    tcp_sent(pcb, NULL);
    tcp_err(pcb, NULL);
    tcp_poll(pcb, NULL, 0);

    tcp_clear_client_slot(client);

    err = tcp_close(pcb);

    if (err != ERR_OK)
    {
        tcp_abort(pcb);
    }

    if (notify_callback != 0)
    {
        TCP_Disconnected_Callback(local_port);
    }
}


/* -------------------------------------------------------------------------- */
/* TCP Callbacks                                                              */
/* -------------------------------------------------------------------------- */

static void tcp_err_callback(void *arg, err_t err)
{
    tcp_client_slot_t *client = (tcp_client_slot_t *)arg;
    uint16_t local_port;

    if (client == NULL)
    {
        return;
    }

    local_port = client->local_port;

    /*
     * Wichtig:
     * Der tcp_pcb ist bei diesem Callback bereits nicht mehr gültig.
     * Deshalb hier kein tcp_close() oder tcp_abort() aufrufen.
     */
    tcp_clear_client_slot(client);

    xil_printf("TCP connection lost on local port %d, err=%d\r\n",
               local_port,
               err);

    TCP_Disconnected_Callback(local_port);
}


static err_t tcp_recv_callback(void *arg,
                               struct tcp_pcb *tpcb,
                               struct pbuf *p,
                               err_t err)
{
    tcp_client_slot_t *client = (tcp_client_slot_t *)arg;
    struct pbuf *q;
    uint16_t local_port;

    /*
     * p == NULL bedeutet:
     * Der Client hat die TCP-Verbindung sauber geschlossen.
     */
    if (p == NULL)
    {
        if ((client != NULL) &&
            (client->used != 0) &&
            (client->pcb == tpcb))
        {
            xil_printf("TCP client disconnected from local port %d\r\n",
                       client->local_port);

            tcp_close_client_slot(client, 1);
        }
        else
        {
            tcp_close(tpcb);
        }

        return ERR_OK;
    }

    /*
     * Bei einem lwIP-Fehler muss der pbuf freigegeben werden.
     */
    if (err != ERR_OK)
    {
        pbuf_free(p);
        return err;
    }

    /*
     * Sollte normalerweise nie passieren.
     */
    if ((client == NULL) ||
        (client->used == 0) ||
        (client->pcb != tpcb))
    {
        pbuf_free(p);
        tcp_abort(tpcb);
        return ERR_ABRT;
    }

    local_port = client->local_port;

    /*
     * TCP ist ein Bytestrom.
     *
     * Ein pbuf ist nicht automatisch ein vollständiges
     * Anwendungs-Telegramm. Bei eigenen Protokollen brauchst du daher
     * bei Bedarf Längenfelder, Header oder Terminierungszeichen.
     */
    q = p;

    while (q != NULL)
    {
        TCP_Receive_Callback(local_port,
                             (uint8_t *)q->payload,
                             q->len);

        q = q->next;
    }

    /*
     * Erst nachdem die Anwendung die Daten verarbeitet hat,
     * wird TCP mitgeteilt, dass der Empfangspuffer frei ist.
     */
    tcp_recved(tpcb, p->tot_len);

    pbuf_free(p);

    /*
     * Erzwingt bei Bedarf eine zeitnahe ACK-Ausgabe.
     * Das verhindert bei manchen Gegenstellen unnötig lange Wartezeiten.
     */
    (void)tcp_output(tpcb);

    return ERR_OK;
}


static err_t tcp_accept_callback(void *arg,
                                 struct tcp_pcb *newpcb,
                                 err_t err)
{
    tcp_listener_slot_t *listener = (tcp_listener_slot_t *)arg;
    tcp_client_slot_t *client;

    if ((err != ERR_OK) || (newpcb == NULL) || (listener == NULL))
    {
        return ERR_VAL;
    }

    client = tcp_find_free_client_slot();

    if (client == NULL)
    {
        xil_printf("TCP connection rejected: no free client slot\r\n");

        tcp_abort(newpcb);
        return ERR_ABRT;
    }

    client->used       = 1;
    client->local_port = listener->local_port;
    client->pcb        = newpcb;

    tcp_arg(newpcb, client);
    tcp_recv(newpcb, tcp_recv_callback);
    tcp_err(newpcb, tcp_err_callback);

    xil_printf("TCP client connected on local port %d\r\n",
               client->local_port);

    TCP_Connected_Callback(client->local_port);

    return ERR_OK;
}


/* -------------------------------------------------------------------------- */
/* TCP Server Ports                                                           */
/* -------------------------------------------------------------------------- */

int TCP_Open_Port(uint16_t port)
{
    struct tcp_pcb *pcb;
    tcp_listener_slot_t *listener;
    err_t err;

    if (g_ethernet_initialized == 0)
    {
        xil_printf("TCP_Open_Port failed: Ethernet not initialized\r\n");
        return -10;
    }

    /*
     * Port ist bereits offen.
     */
    if (tcp_find_listener(port) != NULL)
    {
        return 0;
    }

    listener = tcp_find_free_listener_slot();

    if (listener == NULL)
    {
        xil_printf("TCP_Open_Port failed: no free listener slot\r\n");
        return -11;
    }

    pcb = tcp_new();

    if (pcb == NULL)
    {
        xil_printf("tcp_new failed\r\n");
        return -1;
    }

    err = tcp_bind(pcb, IP_ADDR_ANY, port);

    if (err != ERR_OK)
    {
        xil_printf("tcp_bind failed on port %d: %d\r\n", port, err);

        tcp_abort(pcb);
        return -2;
    }

    pcb = tcp_listen(pcb);

    if (pcb == NULL)
    {
        xil_printf("tcp_listen failed on port %d\r\n", port);
        return -3;
    }

    listener->used       = 1;
    listener->local_port = port;
    listener->listen_pcb = pcb;

    tcp_arg(pcb, listener);
    tcp_accept(pcb, tcp_accept_callback);

    xil_printf("TCP server listening on port %d\r\n", port);

    return 0;
}


int TCP_Close_Port(uint16_t port)
{
    tcp_listener_slot_t *listener;
    uint32_t i;
    err_t err;

    listener = tcp_find_listener(port);

    if (listener == NULL)
    {
        return -1;
    }

    /*
     * Zuerst alle noch verbundenen Clients dieses Ports schließen.
     */
    for (i = 0; i < ETHERNET_MAX_TCP_CLIENTS; i++)
    {
        if ((g_tcp_clients[i].used != 0) &&
            (g_tcp_clients[i].local_port == port))
        {
            tcp_close_client_slot(&g_tcp_clients[i], 1);
        }
    }

    if (listener->listen_pcb != NULL)
    {
        tcp_arg(listener->listen_pcb, NULL);
        tcp_accept(listener->listen_pcb, NULL);

        err = tcp_close(listener->listen_pcb);

        if (err != ERR_OK)
        {
            tcp_abort(listener->listen_pcb);
        }
    }

    memset(listener, 0, sizeof(*listener));

    xil_printf("TCP server port %d closed\r\n", port);

    return 0;
}


int TCP_Port_Is_Open(uint16_t port)
{
    return (tcp_find_listener(port) != NULL) ? 1 : 0;
}


uint16_t TCP_Get_Client_Count(uint16_t port)
{
    uint16_t count = 0;
    uint32_t i;

    for (i = 0; i < ETHERNET_MAX_TCP_CLIENTS; i++)
    {
        if ((g_tcp_clients[i].used != 0) &&
            (g_tcp_clients[i].local_port == port) &&
            (g_tcp_clients[i].pcb != NULL))
        {
            count++;
        }
    }

    return count;
}


/* -------------------------------------------------------------------------- */
/* TCP Senden                                                                 */
/* -------------------------------------------------------------------------- */

static int tcp_send_to_client(tcp_client_slot_t *client,
                              const void *data,
                              uint16_t len)
{
    err_t err;

    if ((client == NULL) ||
        (client->used == 0) ||
        (client->pcb == NULL))
    {
        return -1;
    }

    if ((data == NULL) || (len == 0))
    {
        return -2;
    }


    // ###################################################################################################
    //								This is new and blocks the prograam ...
    while(client->pcb->unsent > 40)
    {
    	//client->pcb->snd_queuelen
    	Ethernet_Process();
    }

    /*
     * tcp_write(..., TCP_WRITE_FLAG_COPY) kopiert die Daten in den
     * lwIP-Sendepuffer. data darf nach dem Funktionsaufruf verändert
     * oder freigegeben werden.
     */
    if (tcp_sndbuf(client->pcb) < len)
    {
        return -3;
    }

    err = tcp_write(client->pcb,
                    data,
                    len,
                    TCP_WRITE_FLAG_COPY);

    if (err != ERR_OK)
    {
        xil_printf("tcp_write failed on port %d: %d\r\n",
                   client->local_port,
                   err);

        return -4;
    }

    err = tcp_output(client->pcb);

    if (err != ERR_OK)
    {
        xil_printf("tcp_output failed on port %d: %d\r\n",
                   client->local_port,
                   err);

        return -5;
    }

    return 0;
}


/*
 * Sendet an alle verbundenen Clients eines lokalen Ports.
 *
 * Bei nur einem Client pro Port entspricht das einer normalen
 * Punkt-zu-Punkt-Übertragung.
 */
int TCP_Send_To_Port(uint16_t local_port,
                     const void *data,
                     uint16_t len)
{
    uint32_t i;
    int sent_count = 0;
    int first_error = -1;
    int found_client = 0;
    int result;

    if ((data == NULL) || (len == 0))
    {
        return -2;
    }

    for (i = 0; i < ETHERNET_MAX_TCP_CLIENTS; i++)
    {
        if ((g_tcp_clients[i].used == 0) ||
            (g_tcp_clients[i].local_port != local_port) ||
            (g_tcp_clients[i].pcb == NULL))
        {
            continue;
        }

        found_client = 1;

        result = tcp_send_to_client(&g_tcp_clients[i], data, len);

        if (result == 0)
        {
            sent_count++;
        }
        else if (first_error == -1)
        {
            first_error = result;
        }
    }

    if (sent_count > 0)
    {
        return sent_count;
    }

    if (found_client == 0)
    {
        xil_printf("TCP send failed: no client on port %d\r\n",
                   local_port);

        return -1;
    }

    return first_error;
}


int TCP_Send_String_To_Port(uint16_t local_port,
                            const char *text)
{
    if (text == NULL)
    {
        return -1;
    }

    return TCP_Send_To_Port(local_port,
                            text,
                            (uint16_t)strlen(text));
}


/*
 * Alte API: weiterhin über den Default-Port 5001 senden.
 */
int tcp_server_send(const void *data, uint16_t len)
{
    int result;

    result = TCP_Send_To_Port(TCP_DEFAULT_SERVER_PORT, data, len);

    return (result > 0) ? 0 : result;
}


int tcp_server_send_string(const char *text)
{
    int result;

    result = TCP_Send_String_To_Port(TCP_DEFAULT_SERVER_PORT, text);

    return (result > 0) ? 0 : result;
}


/* -------------------------------------------------------------------------- */
/* Ethernet Initialisierung                                                   */
/* -------------------------------------------------------------------------- */

int Ethernet_Init(uint8_t ip1,
                  uint8_t ip2,
                  uint8_t ip3,
                  uint8_t ip4)
{
    ip_addr_t ipaddr;
    ip_addr_t netmask;
    ip_addr_t gw;

    /*
     * lwIP und GEM dürfen nicht mehrfach initialisiert werden.
     */
    if (g_ethernet_initialized != 0)
    {
        return 0;
    }

    xil_printf("\r\n--- Zynq PS GEM lwIP ---\r\n");

    Xil_ICacheEnable();
    Xil_DCacheEnable();

    IP4_ADDR(&ipaddr,  ip1, ip2, ip3, ip4);
    IP4_ADDR(&netmask, 255, 255, 255, 0);
    IP4_ADDR(&gw,      192, 168, 1, 1);

    memset(g_tcp_listeners, 0, sizeof(g_tcp_listeners));
    memset(g_tcp_clients,   0, sizeof(g_tcp_clients));

    lwip_init();

    if (!xemac_add(&server_netif,
                   &ipaddr,
                   &netmask,
                   &gw,
                   mac_ethernet_address,
                   PLATFORM_EMAC_BASEADDR))
    {
        xil_printf("ERROR: xemac_add failed\r\n");
        return -1;
    }

    netif_set_default(&server_netif);
    netif_set_up(&server_netif);

    g_ethernet_initialized = 1;

    xil_printf("Board IP:  %d.%d.%d.%d\r\n",
               ip4_addr1(&ipaddr),
               ip4_addr2(&ipaddr),
               ip4_addr3(&ipaddr),
               ip4_addr4(&ipaddr));

    xil_printf("Netmask:   %d.%d.%d.%d\r\n",
               ip4_addr1(&netmask),
               ip4_addr2(&netmask),
               ip4_addr3(&netmask),
               ip4_addr4(&netmask));

    xil_printf("Gateway:   %d.%d.%d.%d\r\n",
               ip4_addr1(&gw),
               ip4_addr2(&gw),
               ip4_addr3(&gw),
               ip4_addr4(&gw));

    return 0;
}


int TCP_Init(uint8_t ip1,
             uint8_t ip2,
             uint8_t ip3,
             uint8_t ip4,
             uint16_t first_port)
{
    int result;

    result = Ethernet_Init(ip1, ip2, ip3, ip4);

    if (result != 0)
    {
        return result;
    }

    return TCP_Open_Port(first_port);
}


void TCP_Test_Init(void)
{
    int result;

    result = TCP_Init(192, 168, 1, 10, TCP_DEFAULT_SERVER_PORT);

    if (result != 0)
    {
        xil_printf("ERROR: TCP server start failed: %d\r\n", result);
    }
}


/* -------------------------------------------------------------------------- */
/* UDP                                                                        */
/* -------------------------------------------------------------------------- */

static struct udp_pcb *g_udp_pcb = NULL;

static ip_addr_t g_udp_last_remote_ip;
static u16_t     g_udp_last_remote_port = 0;
static int       g_udp_have_remote = 0;


static void udp_recv_callback(void *arg,
                              struct udp_pcb *pcb,
                              struct pbuf *p,
                              const ip_addr_t *addr,
                              u16_t port)
{
    (void)arg;

    if ((p == NULL) || (addr == NULL))
    {
        return;
    }

    xil_printf("UDP packet received from %d.%d.%d.%d:%d, len=%d\r\n",
               ip4_addr1(addr),
               ip4_addr2(addr),
               ip4_addr3(addr),
               ip4_addr4(addr),
               port,
               p->tot_len);

    ip_addr_copy(g_udp_last_remote_ip, *addr);
    g_udp_last_remote_port = port;
    g_udp_have_remote = 1;

    /*
     * Optionales UDP-Echo.
     * Diese Zeile entfernen, wenn du nicht zurücksenden möchtest.
     */
    udp_sendto(pcb, p, addr, port);

    pbuf_free(p);
}


static int start_udp_server(uint16_t port)
{
    err_t err;

    if (g_ethernet_initialized == 0)
    {
        xil_printf("UDP start failed: Ethernet not initialized\r\n");
        return -10;
    }

    if (g_udp_pcb != NULL)
    {
        return 0;
    }

    g_udp_pcb = udp_new();

    if (g_udp_pcb == NULL)
    {
        xil_printf("udp_new failed\r\n");
        return -1;
    }

    err = udp_bind(g_udp_pcb, IP_ADDR_ANY, port);

    if (err != ERR_OK)
    {
        xil_printf("udp_bind failed: %d\r\n", err);

        udp_remove(g_udp_pcb);
        g_udp_pcb = NULL;

        return -2;
    }

    udp_recv(g_udp_pcb, udp_recv_callback, NULL);

    xil_printf("UDP server listening on port %d\r\n", port);

    return 0;
}


void UDP_Test_Init(void)
{
    if (start_udp_server(UDP_SERVER_PORT) != 0)
    {
        xil_printf("ERROR: UDP server start failed\r\n");
    }
}


int udp_server_send_to_ip(const void *data,
                          uint16_t len,
                          uint8_t ip0,
                          uint8_t ip1,
                          uint8_t ip2,
                          uint8_t ip3,
                          uint16_t port)
{
    struct pbuf *p;
    ip_addr_t remote_ip;
    err_t err;

    if (g_udp_pcb == NULL)
    {
        xil_printf("UDP send failed: UDP server not initialized\r\n");
        return -1;
    }

    if ((data == NULL) || (len == 0))
    {
        return -2;
    }

    IP4_ADDR(&remote_ip, ip0, ip1, ip2, ip3);

    p = pbuf_alloc(PBUF_TRANSPORT, len, PBUF_RAM);

    if (p == NULL)
    {
        xil_printf("UDP send failed: pbuf_alloc failed\r\n");
        return -3;
    }

    memcpy(p->payload, data, len);

    err = udp_sendto(g_udp_pcb, p, &remote_ip, port);

    pbuf_free(p);

    if (err != ERR_OK)
    {
        xil_printf("udp_sendto failed: %d\r\n", err);
        return -4;
    }

    return 0;
}


/* -------------------------------------------------------------------------- */
/* Zyklische Bearbeitung                                                      */
/* -------------------------------------------------------------------------- */

void Ethernet_Process(void)
{
    if (g_ethernet_initialized == 0)
    {
        return;
    }

    /*
     * Ethernet-Empfang abarbeiten.
     */
    xemacif_input(&server_netif);

    /*
     * TCP-Timer, ACKs, Retransmissions und Timeouts abarbeiten.
     *
     * Nur einmal im System aufrufen. Falls du sys_check_timeouts()
     * bereits in einem eigenen xiltimer-Callback aufrufst, diese Zeile
     * hier entfernen.
     */
    //sys_check_timeouts();
}


/* -------------------------------------------------------------------------- */
/* Weak Application Callbacks                                                 */
/* -------------------------------------------------------------------------- */

__attribute__((weak)) void TCP_Receive_Callback(uint16_t local_port, uint8_t *buffer, uint16_t length)
{
    (void)local_port;
    (void)buffer;
    (void)length;

    /* Eigene Implementierung in einer anderen .c-Datei anlegen. */
}


__attribute__((weak))
void TCP_Connected_Callback(uint16_t local_port)
{
    (void)local_port;
}


__attribute__((weak))
void TCP_Disconnected_Callback(uint16_t local_port)
{
    (void)local_port;
}
