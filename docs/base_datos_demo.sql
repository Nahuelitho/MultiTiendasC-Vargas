-- MariaDB dump 10.17  Distrib 10.4.14-MariaDB, for Win64 (AMD64)
--
-- Host: localhost    Database: tu_tienda_cerca_demo
-- ------------------------------------------------------
-- Server version	10.4.14-MariaDB

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__efmigrationshistory`
--

LOCK TABLES `__efmigrationshistory` WRITE;
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory` VALUES ('20261103012659_MigracionInicial','9.0.0'),('20261103040012_AgregarUsuariosSeed','9.0.0');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `carritoitems`
--

DROP TABLE IF EXISTS `carritoitems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `carritoitems` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `CarritoId` int(11) NOT NULL,
  `EmpresaId` int(11) NOT NULL,
  `ProductoId` int(11) NOT NULL,
  `Nombre` varchar(160) NOT NULL,
  `PrecioUnitario` decimal(12,2) NOT NULL,
  `Cantidad` int(11) NOT NULL,
  `Subtotal` decimal(12,2) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_CarritoItems_CarritoId` (`CarritoId`),
  KEY `IX_CarritoItems_ProductoId` (`ProductoId`),
  CONSTRAINT `FK_CarritoItems_Carritos_CarritoId` FOREIGN KEY (`CarritoId`) REFERENCES `carritos` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_CarritoItems_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `productos` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `carritoitems`
--

LOCK TABLES `carritoitems` WRITE;
/*!40000 ALTER TABLE `carritoitems` DISABLE KEYS */;
/*!40000 ALTER TABLE `carritoitems` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `carritos`
--

DROP TABLE IF EXISTS `carritos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `carritos` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UsuarioId` int(11) NOT NULL,
  `FechaCreacion` datetime(6) NOT NULL,
  `FechaUltimaActualizacion` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Carritos_UsuarioId` (`UsuarioId`),
  CONSTRAINT `FK_Carritos_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `carritos`
--

LOCK TABLES `carritos` WRITE;
/*!40000 ALTER TABLE `carritos` DISABLE KEYS */;
/*!40000 ALTER TABLE `carritos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `denuncias`
--

DROP TABLE IF EXISTS `denuncias`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `denuncias` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UsuarioId` int(11) NOT NULL,
  `EmpresaId` int(11) NOT NULL,
  `ProductoId` int(11) DEFAULT NULL,
  `Motivo` int(11) NOT NULL,
  `Descripcion` varchar(1000) DEFAULT NULL,
  `Estado` int(11) NOT NULL,
  `FechaCreacion` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Denuncias_EmpresaId` (`EmpresaId`),
  KEY `IX_Denuncias_ProductoId` (`ProductoId`),
  KEY `IX_Denuncias_UsuarioId` (`UsuarioId`),
  CONSTRAINT `FK_Denuncias_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `empresas` (`Id`),
  CONSTRAINT `FK_Denuncias_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `productos` (`Id`),
  CONSTRAINT `FK_Denuncias_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `denuncias`
--

LOCK TABLES `denuncias` WRITE;
/*!40000 ALTER TABLE `denuncias` DISABLE KEYS */;
/*!40000 ALTER TABLE `denuncias` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `empresas`
--

DROP TABLE IF EXISTS `empresas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `empresas` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `PropietarioId` int(11) NOT NULL,
  `Nombre` varchar(120) NOT NULL,
  `Descripcion` varchar(500) DEFAULT NULL,
  `Direccion` varchar(150) NOT NULL,
  `Localidad` varchar(100) NOT NULL,
  `Latitud` decimal(9,6) DEFAULT NULL,
  `Longitud` decimal(9,6) DEFAULT NULL,
  `Telefono` varchar(100) DEFAULT NULL,
  `MediosDePago` varchar(250) DEFAULT NULL,
  `OfreceDelivery` tinyint(1) NOT NULL,
  `Estado` int(11) NOT NULL,
  `FechaAlta` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Empresas_PropietarioId` (`PropietarioId`),
  CONSTRAINT `FK_Empresas_Usuarios_PropietarioId` FOREIGN KEY (`PropietarioId`) REFERENCES `usuarios` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `empresas`
--

LOCK TABLES `empresas` WRITE;
/*!40000 ALTER TABLE `empresas` DISABLE KEYS */;
INSERT INTO `empresas` VALUES (1,2002,'Mercado Central','Almacén de barrio con productos de almacén, bebidas y limpieza.','Av. Illia 1250','San Luis',NULL,NULL,'2664-400101','Efectivo, Mercado Pago, Tarjeta débito',1,1,'2026-11-06 03:56:33.581843'),(2,2004,'Panadería La Espiga','Panadería artesanal con pan casero, facturas y chipá recién horneados.','San Martín 845','Juana Koslay',NULL,NULL,'2664-400102','Efectivo, Mercado Pago',1,1,'2026-11-06 03:56:33.889410'),(3,2005,'Farmacia San Luis','Farmacia con medicamentos de venta libre, dermocosmética y primeros auxilios.','Colón 560','San Luis',NULL,NULL,'2664-400103','Efectivo, Tarjeta débito, Tarjeta crédito',0,1,'2026-11-06 03:56:33.914981'),(4,2006,'Tienda del Cerro','Regalos, decoración y artículos regionales para tu hogar.','Ruta 20 km 4','Potrero de los Funes',NULL,NULL,'2664-400104','Efectivo, Mercado Pago, Transferencia',1,1,'2026-11-06 03:56:33.933775'),(5,2007,'Café de la Plaza','Cafetería de especialidad con opciones dulces y saladas para llevar.','Pringles 320','La Punta',NULL,NULL,'2664-400105','Efectivo, Mercado Pago, Tarjeta débito',1,1,'2026-11-06 03:56:33.950981'),(6,2008,'Verdulería El Huerto','Frutas y verduras frescas de productores de la zona.','Belgrano 1180','Villa Mercedes',NULL,NULL,'2664-400106','Efectivo, Transferencia',1,1,'2026-11-06 03:56:33.968832'),(7,2009,'Librería El Saber','Útiles escolares, librería y artículos de oficina.','Ayacucho 670','San Luis',NULL,NULL,'2664-400107','Efectivo, Mercado Pago, Tarjeta débito',0,1,'2026-11-06 03:56:33.986651'),(8,2010,'Boutique Aurora','Indumentaria y accesorios para todos los estilos.','Rivadavia 890','Merlo',NULL,NULL,'2664-400108','Efectivo, Mercado Pago, Tarjeta crédito',1,1,'2026-11-06 03:56:34.005497');
/*!40000 ALTER TABLE `empresas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `imagenesproducto`
--

DROP TABLE IF EXISTS `imagenesproducto`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `imagenesproducto` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `ProductoId` int(11) NOT NULL,
  `RutaArchivo` varchar(250) NOT NULL,
  `EsPrincipal` tinyint(1) NOT NULL,
  `FechaCarga` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ImagenesProducto_ProductoId` (`ProductoId`),
  CONSTRAINT `FK_ImagenesProducto_Productos_ProductoId` FOREIGN KEY (`ProductoId`) REFERENCES `productos` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `imagenesproducto`
--

LOCK TABLES `imagenesproducto` WRITE;
/*!40000 ALTER TABLE `imagenesproducto` DISABLE KEYS */;
/*!40000 ALTER TABLE `imagenesproducto` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `notificaciones`
--

DROP TABLE IF EXISTS `notificaciones`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `notificaciones` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UsuarioId` int(11) NOT NULL,
  `EmpresaId` int(11) NOT NULL,
  `PedidoId` int(11) DEFAULT NULL,
  `Titulo` varchar(160) NOT NULL,
  `Mensaje` varchar(500) NOT NULL,
  `Leida` tinyint(1) NOT NULL,
  `Fecha` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Notificaciones_EmpresaId` (`EmpresaId`),
  KEY `IX_Notificaciones_PedidoId` (`PedidoId`),
  KEY `IX_Notificaciones_UsuarioId` (`UsuarioId`),
  CONSTRAINT `FK_Notificaciones_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `empresas` (`Id`),
  CONSTRAINT `FK_Notificaciones_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `pedidos` (`Id`),
  CONSTRAINT `FK_Notificaciones_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notificaciones`
--

LOCK TABLES `notificaciones` WRITE;
/*!40000 ALTER TABLE `notificaciones` DISABLE KEYS */;
/*!40000 ALTER TABLE `notificaciones` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pedidoitems`
--

DROP TABLE IF EXISTS `pedidoitems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `pedidoitems` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `PedidoId` int(11) NOT NULL,
  `EmpresaId` int(11) NOT NULL,
  `ProductoId` int(11) DEFAULT NULL,
  `Nombre` varchar(160) NOT NULL,
  `PrecioUnitario` decimal(12,2) NOT NULL,
  `Cantidad` int(11) NOT NULL,
  `Subtotal` decimal(12,2) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_PedidoItems_PedidoId_EmpresaId` (`PedidoId`,`EmpresaId`),
  KEY `IX_PedidoItems_ProductoId_EmpresaId` (`ProductoId`,`EmpresaId`),
  CONSTRAINT `FK_PedidoItems_Pedidos_PedidoId_EmpresaId` FOREIGN KEY (`PedidoId`, `EmpresaId`) REFERENCES `pedidos` (`Id`, `EmpresaId`) ON DELETE CASCADE,
  CONSTRAINT `FK_PedidoItems_Productos_ProductoId_EmpresaId` FOREIGN KEY (`ProductoId`, `EmpresaId`) REFERENCES `productos` (`Id`, `EmpresaId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pedidoitems`
--

LOCK TABLES `pedidoitems` WRITE;
/*!40000 ALTER TABLE `pedidoitems` DISABLE KEYS */;
/*!40000 ALTER TABLE `pedidoitems` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pedidos`
--

DROP TABLE IF EXISTS `pedidos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `pedidos` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Codigo` varchar(20) NOT NULL,
  `UsuarioId` int(11) NOT NULL,
  `EmpresaId` int(11) NOT NULL,
  `GrupoPedidoId` int(11) DEFAULT NULL,
  `Fecha` datetime(6) NOT NULL,
  `Estado` varchar(30) NOT NULL,
  `Total` decimal(12,2) NOT NULL,
  `DireccionEntrega` varchar(160) DEFAULT NULL,
  `TelefonoContacto` varchar(50) DEFAULT NULL,
  `FormaPago` varchar(50) DEFAULT NULL,
  `Notas` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_Pedidos_Id_EmpresaId` (`Id`,`EmpresaId`),
  KEY `IX_Pedidos_EmpresaId` (`EmpresaId`),
  KEY `IX_Pedidos_GrupoPedidoId` (`GrupoPedidoId`),
  KEY `IX_Pedidos_UsuarioId` (`UsuarioId`),
  CONSTRAINT `FK_Pedidos_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `empresas` (`Id`),
  CONSTRAINT `FK_Pedidos_Pedidos_GrupoPedidoId` FOREIGN KEY (`GrupoPedidoId`) REFERENCES `pedidos` (`Id`),
  CONSTRAINT `FK_Pedidos_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`),
  CONSTRAINT `CK_Pedidos_Estado` CHECK (`Estado` in ('Pendiente','Confirmado','EnPreparacion','Listo','Entregado','Cancelado'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pedidos`
--

LOCK TABLES `pedidos` WRITE;
/*!40000 ALTER TABLE `pedidos` DISABLE KEYS */;
/*!40000 ALTER TABLE `pedidos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `productos`
--

DROP TABLE IF EXISTS `productos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `productos` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `EmpresaId` int(11) NOT NULL,
  `Nombre` varchar(150) NOT NULL,
  `Descripcion` varchar(500) DEFAULT NULL,
  `Precio` decimal(10,2) NOT NULL,
  `Stock` int(11) NOT NULL,
  `Activo` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AK_Productos_Id_EmpresaId` (`Id`,`EmpresaId`),
  KEY `IX_Productos_EmpresaId` (`EmpresaId`),
  CONSTRAINT `FK_Productos_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `empresas` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=54 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `productos`
--

LOCK TABLES `productos` WRITE;
/*!40000 ALTER TABLE `productos` DISABLE KEYS */;
INSERT INTO `productos` VALUES (1,1,'Yerba Mate Taragüí 1 kg','Yerba mate despalillada suave.',4500.00,25,1),(2,1,'Café Molido La Virginia 500 g','Café tostado molido clásico.',6200.00,18,1),(3,1,'Azúcar Ledesma 1 kg','Azúcar refinada en paquete de 1 kg.',1800.00,40,1),(4,1,'Aceite de Girasol Natura 1.5 L','Aceite de girasol para cocina diaria.',3200.00,22,1),(5,1,'Leche Entera La Serenísima 1 L','Leche entera larga vida.',1500.00,35,1),(6,1,'Fideos Don Vicente 500 g','Fideos tallarín n° 5.',1200.00,50,1),(7,1,'Arroz Gallo Oro 1 kg','Arroz largo fino parboilizado.',2100.00,30,1),(8,1,'Harina Cañuelas 000 1 kg','Harina de trigo para panificados.',1400.00,28,1),(9,1,'Galletitas Oreo 118 g','Galletitas de chocolate rellenas.',1100.00,45,1),(10,1,'Gaseosa Coca-Cola 2.25 L','Bebida gaseosa sabor cola.',3500.00,20,1),(11,1,'Detergente Ala 750 ml','Detergente concentrado limón.',2800.00,16,1),(12,1,'Papel Higiénico Elite 4 rollos','Papel higiénico doble hoja.',3600.00,24,1),(13,2,'Pan Casero 1 kg','Pan de campo recién horneado.',2500.00,30,1),(14,2,'Medialunas (docena)','Medialunas de manteca.',4800.00,20,1),(15,2,'Facturas Surtidas (docena)','Surtido de facturas de la casa.',5200.00,18,1),(16,2,'Bizcochuelo Casero','Bizcochuelo esponjoso para tortas.',3500.00,10,1),(17,2,'Pan Integral 800 g','Pan integral con semillas.',3000.00,15,1),(18,2,'Chipá (docena)','Chipá de queso al horno.',4200.00,12,1),(19,3,'Paracetamol 500 mg x 20','Analgésico y antifebril.',2500.00,40,1),(20,3,'Ibuprofeno 400 mg x 20','Antiinflamatorio de venta libre.',3200.00,35,1),(21,3,'Alcohol en Gel 250 ml','Alcohol en gel con glicerina.',1900.00,50,1),(22,3,'Protector Solar FPS 30','Protección solar facial y corporal.',8500.00,14,1),(23,3,'Vitamina C Efervescente','Comprimidos efervescentes sabor naranja.',2800.00,26,1),(24,3,'Curitas x 20','Apósitos adhesivos de distintos tamaños.',1200.00,60,1),(25,4,'Mate de Algarrobo','Mate tallado en madera de algarrobo.',9500.00,8,1),(26,4,'Set de Tazas Cerámicas','Juego de 4 tazas artesanales.',12000.00,6,1),(27,4,'Lámpara de Escritorio','Lámpara LED con brazo articulado.',18500.00,5,1),(28,4,'Cuadro Decorativo','Cuadro con paisaje serrano en lienzo.',7800.00,9,1),(29,4,'Billetera de Cuero','Billetera artesanal de cuero vacuno.',14200.00,7,1),(30,5,'Café Americano','Café de especialidad recién preparado.',2200.00,30,1),(31,5,'Cappuccino','Espresso con leche vaporizada y espuma.',2800.00,25,1),(32,5,'Té Verde','Té verde en hebras.',1900.00,20,1),(33,5,'Cheesecake porción','Cheesecake de frutos rojos.',3200.00,12,1),(34,5,'Tostado de jamón y queso','Pan de miga tostado.',3500.00,15,1),(35,5,'Jugo de naranja','Jugo natural exprimido al momento.',2500.00,18,1),(36,6,'Tomate Perita 1 kg','Tomate perita fresco.',1800.00,40,1),(37,6,'Papa Negra 1 kg','Papa negra de calidad.',1200.00,60,1),(38,6,'Manzana Red Delicious 1 kg','Manzana dulce y crocante.',2200.00,35,1),(39,6,'Zanahoria 1 kg','Zanahoria fresca de huerta.',1400.00,45,1),(40,6,'Lechuga Mantecosa','Lechuga mantecosa de hoja tierna.',900.00,25,1),(41,6,'Naranja 1 kg','Naranja para jugo.',1600.00,50,1),(42,7,'Cuaderno Rivadavia 84 hojas','Cuaderno universitario rayado.',2400.00,50,1),(43,7,'Set de Lapiceras','Set de 3 lapiceras de colores.',3600.00,30,1),(44,7,'Mochila Escolar','Mochila resistente con compartimentos.',18500.00,10,1),(45,7,'Resma A4 500 hojas','Papel blanco A4 75 g.',6500.00,20,1),(46,7,'Calculadora Científica','Calculadora con funciones científicas.',9800.00,12,1),(47,7,'Agenda 2027','Agenda anual tapa dura.',7200.00,14,1),(48,8,'Remera Básica de Algodón','Remera unisex de algodón peinado.',8900.00,24,1),(49,8,'Jean Mom Fit','Jean de tiro alto y calce relaxed.',24500.00,12,1),(50,8,'Vestido Floreado','Vestido liviano con estampado floral.',28900.00,8,1),(51,8,'Buzo Canguro','Buzo frizado con capucha.',19900.00,15,1),(52,8,'Zapatillas Urbanas','Zapatillas livianas para uso diario.',32900.00,9,1),(53,8,'Cartera Pequeña','Cartera de mano con correa.',15900.00,11,1);
/*!40000 ALTER TABLE `productos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `roles` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(40) NOT NULL,
  `Codigo` varchar(40) NOT NULL,
  `Activo` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (1,'Super Administrador','SUPER_ADMIN',1),(2,'Administrador','ADMIN',1),(3,'Comerciante','COMERCIANTE',1),(4,'Cliente','CLIENTE',1);
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarioroles`
--

DROP TABLE IF EXISTS `usuarioroles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `usuarioroles` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UsuarioId` int(11) NOT NULL,
  `RolId` int(11) NOT NULL,
  `EmpresaId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_UsuarioRoles_UsuarioId_RolId_EmpresaId` (`UsuarioId`,`RolId`,`EmpresaId`),
  KEY `IX_UsuarioRoles_EmpresaId` (`EmpresaId`),
  KEY `IX_UsuarioRoles_RolId` (`RolId`),
  CONSTRAINT `FK_UsuarioRoles_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `empresas` (`Id`),
  CONSTRAINT `FK_UsuarioRoles_Roles_RolId` FOREIGN KEY (`RolId`) REFERENCES `roles` (`Id`),
  CONSTRAINT `FK_UsuarioRoles_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2011 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarioroles`
--

LOCK TABLES `usuarioroles` WRITE;
/*!40000 ALTER TABLE `usuarioroles` DISABLE KEYS */;
INSERT INTO `usuarioroles` VALUES (1000,1000,1,NULL),(2001,2001,2,NULL),(2002,2002,3,1),(2003,2003,4,NULL),(2004,2004,3,2),(2005,2005,3,3),(2006,2006,3,4),(2007,2007,3,5),(2008,2008,3,6),(2009,2009,3,7),(2010,2010,3,8);
/*!40000 ALTER TABLE `usuarioroles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `usuarios` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) NOT NULL,
  `Correo` varchar(150) NOT NULL,
  `ContrasenaHash` longtext NOT NULL,
  `AvatarUrl` varchar(250) DEFAULT NULL,
  `FechaAlta` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Usuarios_Correo` (`Correo`)
) ENGINE=InnoDB AUTO_INCREMENT=2011 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
INSERT INTO `usuarios` VALUES (1000,'Super Admin','SuperAdmin@test.com','pbkdf2-sha256$210000$GNnBg2xdU59yCbUaTEKYOw==$jOj6rSsxuMLBYRfhT5CqtpkgiKeFLZn5ydpCcjiJ748=',NULL,'2026-11-02 00:00:00.000000'),(2001,'Administrador','Admin1@test.com','pbkdf2-sha256$210000$VqcoBR6CgScnSYI5t7ZGEQ==$qoAJWmanNZegflVRvb5K5zVLTjRVn3LCD1bOUdrPVTk=',NULL,'2026-11-03 00:00:00.000000'),(2002,'Comerciante','Comerciante1@test.com','pbkdf2-sha256$210000$L63pRxgtk0IjnQGcaoDF1A==$gxTl2eZdKlFtFEfvOoRo4yXsYJle3MjO2nSEMxlRbHs=',NULL,'2026-11-03 00:00:00.000000'),(2003,'Usuario Cliente','Usuario1@test.com','pbkdf2-sha256$210000$v9lrhnUsQzRx+7pkkWFmiQ==$PXLHlMYJoqEZwCn1puanbLhpSHMklR2WsepDgldaI5g=',NULL,'2026-11-03 00:00:00.000000'),(2004,'Comerciante Panadería La Espiga','Comerciante2@test.com','pbkdf2-sha256$210000$Vymme/ssryCL6aXbChf89w==$Z0RXiBkB66aqjwjmVXQR501UKvtGnqRnIJDX60LHhVE=',NULL,'2026-11-06 03:56:33.880143'),(2005,'Comerciante Farmacia San Luis','Comerciante3@test.com','pbkdf2-sha256$210000$Vymme/ssryCL6aXbChf89w==$Z0RXiBkB66aqjwjmVXQR501UKvtGnqRnIJDX60LHhVE=',NULL,'2026-11-06 03:56:33.911884'),(2006,'Comerciante Tienda del Cerro','Comerciante4@test.com','pbkdf2-sha256$210000$Vymme/ssryCL6aXbChf89w==$Z0RXiBkB66aqjwjmVXQR501UKvtGnqRnIJDX60LHhVE=',NULL,'2026-11-06 03:56:33.930812'),(2007,'Comerciante Café de la Plaza','Comerciante5@test.com','pbkdf2-sha256$210000$Vymme/ssryCL6aXbChf89w==$Z0RXiBkB66aqjwjmVXQR501UKvtGnqRnIJDX60LHhVE=',NULL,'2026-11-06 03:56:33.947748'),(2008,'Comerciante Verdulería El Huerto','Comerciante6@test.com','pbkdf2-sha256$210000$Vymme/ssryCL6aXbChf89w==$Z0RXiBkB66aqjwjmVXQR501UKvtGnqRnIJDX60LHhVE=',NULL,'2026-11-06 03:56:33.965517'),(2009,'Comerciante Librería El Saber','Comerciante7@test.com','pbkdf2-sha256$210000$Vymme/ssryCL6aXbChf89w==$Z0RXiBkB66aqjwjmVXQR501UKvtGnqRnIJDX60LHhVE=',NULL,'2026-11-06 03:56:33.983272'),(2010,'Comerciante Boutique Aurora','Comerciante8@test.com','pbkdf2-sha256$210000$Vymme/ssryCL6aXbChf89w==$Z0RXiBkB66aqjwjmVXQR501UKvtGnqRnIJDX60LHhVE=',NULL,'2026-11-06 03:56:34.001864');
/*!40000 ALTER TABLE `usuarios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `valoraciones`
--

DROP TABLE IF EXISTS `valoraciones`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `valoraciones` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UsuarioId` int(11) NOT NULL,
  `EmpresaId` int(11) NOT NULL,
  `Puntuacion` int(11) NOT NULL,
  `Comentario` varchar(500) DEFAULT NULL,
  `FechaCreacion` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Valoraciones_UsuarioId_EmpresaId` (`UsuarioId`,`EmpresaId`),
  KEY `IX_Valoraciones_EmpresaId` (`EmpresaId`),
  CONSTRAINT `FK_Valoraciones_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `empresas` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Valoraciones_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `CK_Valoraciones_Puntuacion` CHECK (`Puntuacion` between 1 and 5)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `valoraciones`
--

LOCK TABLES `valoraciones` WRITE;
/*!40000 ALTER TABLE `valoraciones` DISABLE KEYS */;
INSERT INTO `valoraciones` VALUES (1,2003,1,5,'Excelente atención y buenos productos.','2026-11-06 03:56:33.821165'),(2,2003,2,5,'Excelente atención y buenos productos.','2026-11-06 03:56:33.899495'),(3,2003,3,4,'Excelente atención y buenos productos.','2026-11-06 03:56:33.922211'),(4,2003,4,4,'Excelente atención y buenos productos.','2026-11-06 03:56:33.940807'),(5,2003,5,5,'Excelente atención y buenos productos.','2026-11-06 03:56:33.958492'),(6,2003,6,4,'Excelente atención y buenos productos.','2026-11-06 03:56:33.976045'),(7,2003,7,4,'Excelente atención y buenos productos.','2026-11-06 03:56:33.994289'),(8,2003,8,5,'Excelente atención y buenos productos.','2026-11-06 03:56:34.013205');
/*!40000 ALTER TABLE `valoraciones` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'tu_tienda_cerca_demo'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-11-06  0:58:29
