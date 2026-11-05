-- MySQL dump 10.13  Distrib 8.0.19, for Win64 (x86_64)
--
-- Host: localhost    Database: gimnasio
-- ------------------------------------------------------
-- Server version	8.0.46

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `actividad`
--

DROP TABLE IF EXISTS `actividad`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `actividad` (
  `IdActividad` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) NOT NULL,
  `Estado` tinyint(1) NOT NULL DEFAULT '1',
  `Archivo` varchar(100) DEFAULT NULL,
  `AccesoLibre` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`IdActividad`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `actividad`
--

LOCK TABLES `actividad` WRITE;
/*!40000 ALTER TABLE `actividad` DISABLE KEYS */;
INSERT INTO `actividad` VALUES (1,'Musculacion',1,NULL,1),(2,'Pilates',1,NULL,0),(3,'Zumba',1,NULL,0),(4,'Spinning',1,NULL,0),(5,'Boxeo',1,NULL,0),(6,'Funcional',0,NULL,0),(7,'Yoga',1,NULL,0);
/*!40000 ALTER TABLE `actividad` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `actividad_archivo`
--

DROP TABLE IF EXISTS `actividad_archivo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `actividad_archivo` (
  `IdArchivo` int NOT NULL AUTO_INCREMENT,
  `IdActividad` int NOT NULL,
  `Ruta` varchar(255) NOT NULL,
  PRIMARY KEY (`IdArchivo`),
  KEY `fk_actividad_archivo_actividad` (`IdActividad`),
  CONSTRAINT `fk_actividad_archivo_actividad` FOREIGN KEY (`IdActividad`) REFERENCES `actividad` (`IdActividad`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `actividad_archivo`
--

LOCK TABLES `actividad_archivo` WRITE;
/*!40000 ALTER TABLE `actividad_archivo` DISABLE KEYS */;
INSERT INTO `actividad_archivo` VALUES (1,5,'/imagenes/actividades/63f98d19-b04c-43ff-8081-49fc432cfbc0.mp4'),(2,6,'/imagenes/actividades/3b1e3f17-a92f-445b-990c-2115a50fe09c.mp4'),(3,1,'/imagenes/actividades/47ec9ec7-69e2-4c16-b096-67d8bfbcfa68.jpg'),(4,2,'/imagenes/actividades/2d67b743-9380-4400-acd3-1866b6dbc2b4.jpg'),(5,1,'/imagenes/actividades/a6fe0254-06ce-42e6-bb89-f9554e873475.jpg'),(6,4,'/imagenes/actividades/dbede629-87fe-4e99-865f-cd17fcdd8925.jpg'),(7,7,'/imagenes/actividades/de31db2f-5c5a-4523-9411-74015c2f2c42.mp4'),(8,3,'/imagenes/actividades/f0053c54-5cae-427f-ac14-611227a5116d.jpg'),(9,5,'/imagenes/actividades/d4672f8a-0c99-4bfc-8a62-6ee82c902b8a.jpg');
/*!40000 ALTER TABLE `actividad_archivo` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `asistencia`
--

DROP TABLE IF EXISTS `asistencia`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `asistencia` (
  `IdAsistencia` int NOT NULL AUTO_INCREMENT,
  `IdSocio` int NOT NULL,
  `IdClase` int DEFAULT NULL,
  `FechaIngreso` datetime NOT NULL,
  `Tipo` int NOT NULL,
  PRIMARY KEY (`IdAsistencia`),
  KEY `SocioId` (`IdSocio`),
  KEY `ClaseId` (`IdClase`),
  CONSTRAINT `asistencia_ibfk_1` FOREIGN KEY (`IdSocio`) REFERENCES `socio` (`IdSocio`),
  CONSTRAINT `asistencia_ibfk_2` FOREIGN KEY (`IdClase`) REFERENCES `clase` (`IdClase`),
  CONSTRAINT `chk_asistencia_tipo_clase` CHECK ((((`Tipo` = 1) and (`IdClase` is not null)) or ((`Tipo` = 2) and (`IdClase` is null))))
) ENGINE=InnoDB AUTO_INCREMENT=28 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `asistencia`
--

LOCK TABLES `asistencia` WRITE;
/*!40000 ALTER TABLE `asistencia` DISABLE KEYS */;
INSERT INTO `asistencia` VALUES (1,36,1,'2026-08-03 10:30:00',1),(2,36,3,'2026-08-10 14:00:00',1),(3,36,NULL,'2026-08-18 18:30:00',2),(4,36,5,'2026-08-25 09:00:00',1),(5,36,6,'2026-09-03 17:15:00',1),(6,36,7,'2026-09-11 11:00:00',1),(7,36,8,'2026-09-18 16:20:00',1),(8,36,NULL,'2026-09-25 19:00:00',2),(9,36,10,'2026-10-04 08:30:00',1),(10,36,12,'2026-10-12 15:45:00',1),(11,36,13,'2026-10-19 10:00:00',1),(12,36,15,'2026-10-26 17:00:00',1),(13,36,18,'2026-11-05 14:00:00',1),(14,36,20,'2026-11-05 15:30:00',1),(15,24,1,'2026-09-05 09:00:00',1),(16,24,3,'2026-09-20 18:00:00',1),(17,24,5,'2026-10-10 10:00:00',1),(18,24,6,'2026-11-02 16:00:00',1),(19,24,8,'2026-11-04 11:00:00',1),(20,31,17,'2026-11-05 19:44:33',1),(21,33,NULL,'2026-11-05 19:44:56',2),(22,32,17,'2026-11-05 19:45:22',1),(23,24,NULL,'2026-11-05 19:45:41',2),(24,36,NULL,'2026-11-05 19:47:34',2),(25,36,17,'2026-11-05 19:47:43',1),(26,29,NULL,'2026-11-05 19:50:48',2),(27,29,17,'2026-11-05 19:50:56',1);
/*!40000 ALTER TABLE `asistencia` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `clase`
--

DROP TABLE IF EXISTS `clase`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clase` (
  `IdClase` int NOT NULL AUTO_INCREMENT,
  `IdActividad` int NOT NULL,
  `IdProfesor` int NOT NULL,
  `Dia` int NOT NULL,
  `HoraInicio` time NOT NULL,
  `HoraFin` time NOT NULL,
  `Estado` tinyint DEFAULT NULL,
  PRIMARY KEY (`IdClase`),
  KEY `ActividadId` (`IdActividad`),
  KEY `ProfesorId` (`IdProfesor`),
  CONSTRAINT `clase_ibfk_1` FOREIGN KEY (`IdActividad`) REFERENCES `actividad` (`IdActividad`),
  CONSTRAINT `clase_ibfk_2` FOREIGN KEY (`IdProfesor`) REFERENCES `profesor` (`IdProfesor`)
) ENGINE=InnoDB AUTO_INCREMENT=29 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `clase`
--

LOCK TABLES `clase` WRITE;
/*!40000 ALTER TABLE `clase` DISABLE KEYS */;
INSERT INTO `clase` VALUES (1,2,1,1,'08:00:00','09:00:00',1),(3,6,3,1,'18:00:00','19:00:00',1),(4,3,4,1,'19:00:00','20:00:00',1),(5,7,5,1,'20:00:00','21:00:00',1),(6,4,2,2,'08:00:00','09:00:00',1),(7,5,4,2,'17:00:00','18:00:00',1),(8,3,1,2,'18:00:00','19:00:00',1),(9,6,3,2,'19:00:00','20:00:00',1),(10,2,5,3,'08:00:00','09:00:00',1),(12,7,5,3,'18:00:00','19:00:00',1),(13,4,1,3,'19:00:00','20:00:00',1),(14,5,4,4,'08:00:00','09:00:00',1),(15,6,3,4,'17:00:00','18:00:00',1),(16,3,4,4,'18:00:00','19:00:00',1),(17,2,1,4,'19:00:00','20:00:00',1),(18,7,5,5,'08:00:00','09:00:00',1),(19,4,2,5,'17:00:00','18:00:00',1),(20,6,3,5,'18:00:00','19:00:00',1),(21,3,4,5,'19:00:00','20:00:00',1),(23,5,4,6,'10:00:00','11:00:00',1),(24,6,3,6,'17:00:00','18:00:00',1),(25,7,5,6,'18:00:00','19:00:00',1),(27,2,1,0,'10:00:00','11:00:00',0),(28,6,3,0,'18:00:00','19:00:00',0);
/*!40000 ALTER TABLE `clase` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `membresia`
--

DROP TABLE IF EXISTS `membresia`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `membresia` (
  `IdMembresia` int NOT NULL AUTO_INCREMENT,
  `IdSocio` int NOT NULL,
  `IdPlan` int NOT NULL,
  `FechaInicio` date NOT NULL,
  `FechaFin` date NOT NULL,
  `PrecioContratado` decimal(10,2) NOT NULL,
  `Estado` tinyint(1) NOT NULL DEFAULT '1',
  `IdUsuarioCreador` int NOT NULL,
  `Pagada` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`IdMembresia`),
  KEY `SocioId` (`IdSocio`),
  KEY `PlanId` (`IdPlan`),
  KEY `CreadoPor` (`IdUsuarioCreador`),
  CONSTRAINT `membresia_ibfk_1` FOREIGN KEY (`IdSocio`) REFERENCES `socio` (`IdSocio`),
  CONSTRAINT `membresia_ibfk_2` FOREIGN KEY (`IdPlan`) REFERENCES `plan` (`IdPlan`),
  CONSTRAINT `membresia_ibfk_3` FOREIGN KEY (`IdUsuarioCreador`) REFERENCES `usuario` (`IdUsuario`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `membresia`
--

LOCK TABLES `membresia` WRITE;
/*!40000 ALTER TABLE `membresia` DISABLE KEYS */;
INSERT INTO `membresia` VALUES (1,36,4,'2026-11-05','2026-12-05',24000.00,1,37,1),(2,24,2,'2026-11-01','2026-12-01',19000.00,1,37,1),(3,32,2,'2026-11-05','2026-12-04',19000.00,1,42,1),(4,33,3,'2026-11-05','2026-12-04',15000.00,1,42,1),(5,31,7,'2026-11-05','2026-12-04',9000.00,1,42,1),(6,29,5,'2026-11-05','2026-12-04',32000.00,1,42,1),(7,22,6,'2026-11-05','2026-12-04',17000.00,1,42,1),(8,23,2,'2026-11-05','2026-11-04',19000.00,1,35,0),(9,34,5,'2026-11-05','2026-12-04',32000.00,1,35,1),(10,30,1,'2026-11-05','2026-12-04',28000.00,1,35,1),(11,21,7,'2026-11-05','2026-12-04',9000.00,1,35,1),(12,35,5,'2026-11-05','2026-11-04',32000.00,1,35,0);
/*!40000 ALTER TABLE `membresia` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pago`
--

DROP TABLE IF EXISTS `pago`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pago` (
  `IdPago` int NOT NULL AUTO_INCREMENT,
  `IdMembresia` int NOT NULL,
  `Fecha` datetime NOT NULL,
  `Importe` decimal(10,2) NOT NULL,
  `MedioPago` int NOT NULL,
  `Estado` tinyint(1) NOT NULL DEFAULT '1',
  `IdUsuarioCreador` int NOT NULL,
  `IdUsuarioAnulador` int DEFAULT NULL,
  PRIMARY KEY (`IdPago`),
  KEY `MembresiaId` (`IdMembresia`),
  KEY `RegistradoPor` (`IdUsuarioCreador`),
  KEY `AnuladoPor` (`IdUsuarioAnulador`),
  CONSTRAINT `pago_ibfk_1` FOREIGN KEY (`IdMembresia`) REFERENCES `membresia` (`IdMembresia`),
  CONSTRAINT `pago_ibfk_2` FOREIGN KEY (`IdUsuarioCreador`) REFERENCES `usuario` (`IdUsuario`),
  CONSTRAINT `pago_ibfk_3` FOREIGN KEY (`IdUsuarioAnulador`) REFERENCES `usuario` (`IdUsuario`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pago`
--

LOCK TABLES `pago` WRITE;
/*!40000 ALTER TABLE `pago` DISABLE KEYS */;
INSERT INTO `pago` VALUES (1,1,'2026-08-01 09:00:00',24000.00,1,1,37,NULL),(2,1,'2026-09-01 09:00:00',24000.00,1,1,37,NULL),(3,1,'2026-10-01 09:00:00',24000.00,1,1,37,NULL),(4,1,'2026-11-05 12:45:00',24000.00,1,1,37,NULL),(5,2,'2026-09-01 10:00:00',19000.00,1,1,37,NULL),(6,2,'2026-10-01 10:00:00',19000.00,1,1,37,NULL),(7,2,'2026-11-01 10:00:00',19000.00,1,1,37,NULL),(8,3,'2026-11-05 18:50:45',19000.00,1,1,42,NULL),(9,4,'2026-11-05 18:51:04',15000.00,1,1,42,NULL),(10,5,'2026-11-05 18:51:35',9000.00,1,1,42,NULL),(11,6,'2026-11-05 19:50:32',32000.00,1,1,35,NULL),(12,7,'2026-11-05 19:50:37',17000.00,4,1,35,NULL),(13,9,'2026-11-05 19:53:39',32000.00,3,1,35,NULL),(14,10,'2026-11-05 19:54:03',28000.00,2,1,35,NULL),(15,11,'2026-11-05 19:54:23',9000.00,4,1,35,NULL);
/*!40000 ALTER TABLE `pago` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `plan`
--

DROP TABLE IF EXISTS `plan`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `plan` (
  `IdPlan` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) NOT NULL,
  `UtilizacionesMensuales` int DEFAULT NULL,
  `Precio` decimal(10,2) NOT NULL,
  `Estado` tinyint(1) NOT NULL DEFAULT '1',
  `EsIlimitado` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`IdPlan`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `plan`
--

LOCK TABLES `plan` WRITE;
/*!40000 ALTER TABLE `plan` DISABLE KEYS */;
INSERT INTO `plan` VALUES (1,'Musculacion libre',NULL,28000.00,1,1),(2,'Musculacion 12 usos',12,19000.00,1,0),(3,'Musculacion 8 usos',8,15000.00,1,0),(4,'Clases grupales ilimitadas',NULL,24000.00,1,1),(5,'Plan combinado',16,32000.00,1,0),(6,'Plan estudiante',12,17000.00,1,0),(7,'Pase semanal',NULL,9000.00,1,1);
/*!40000 ALTER TABLE `plan` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `profesor`
--

DROP TABLE IF EXISTS `profesor`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `profesor` (
  `IdProfesor` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) NOT NULL,
  `Apellido` varchar(100) NOT NULL,
  `Dni` varchar(20) NOT NULL,
  `Telefono` varchar(50) NOT NULL,
  `Domicilio` varchar(200) NOT NULL,
  `Estado` tinyint NOT NULL,
  PRIMARY KEY (`IdProfesor`),
  UNIQUE KEY `Dni` (`Dni`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `profesor`
--

LOCK TABLES `profesor` WRITE;
/*!40000 ALTER TABLE `profesor` DISABLE KEYS */;
INSERT INTO `profesor` VALUES (1,'Carolina','Alvarez','30111222','2664211001','Av. España 120, San Luis',1),(2,'Diego','Ramirez','30222333','2664211002','Junin 450, San Luis',1),(3,'Florencia','Quiroga','30333444','2664211003','Colon 630, San Luis',1),(4,'Martin','Sosa','30444555','2664211004','Pringles 780, San Luis',1),(5,'Rocio','Ponce','30555666','2664211005','Las Heras 310, San Luis',1);
/*!40000 ALTER TABLE `profesor` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `socio`
--

DROP TABLE IF EXISTS `socio`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `socio` (
  `IdSocio` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) NOT NULL,
  `Apellido` varchar(100) NOT NULL,
  `Dni` varchar(20) NOT NULL,
  `Telefono` varchar(50) NOT NULL,
  `Domicilio` varchar(200) NOT NULL,
  `Estado` tinyint(1) NOT NULL DEFAULT '1',
  `idUsuario` int DEFAULT NULL,
  PRIMARY KEY (`IdSocio`),
  UNIQUE KEY `Dni` (`Dni`),
  KEY `fk_socio_usuario` (`idUsuario`),
  CONSTRAINT `fk_socio_usuario` FOREIGN KEY (`idUsuario`) REFERENCES `usuario` (`IdUsuario`)
) ENGINE=InnoDB AUTO_INCREMENT=37 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `socio`
--

LOCK TABLES `socio` WRITE;
/*!40000 ALTER TABLE `socio` DISABLE KEYS */;
INSERT INTO `socio` VALUES (21,'juan','torres','87987654','2665988754','Las Heras 159',1,27),(22,'romina','florez','54987654','2665489865','colon 456',1,28),(23,'pablo','gimenez','87654321','2665983265','mitre 900',1,29),(24,'tamara','peña','32654987','2664985421','ilia 753',1,30),(25,'tomas','del campo','32654159','2664987321','calle 12',1,31),(26,'florencia','quiroga','65147852','2664987321','juana koslay 654',1,32),(27,'jonathan','delicia','98654321','2664875432','la punta',1,33),(28,'kevin','ballerini','32951654','266498654258','mitre123',1,34),(29,'brian','delicia','98654987','2664653221','la punta',1,35),(30,'yamila','torres','95654357','2665988754','las heras 555',1,36),(31,'gabriela','ballerini','54658987','2665988754','san luis 123',1,37),(32,'fatima','alcaraz','98654025','2665987321','ilia 496',1,38),(33,'fernando','alcaraz','32654985','2665322165','ituzaigo',1,39),(34,'jose','perez','21326987','2665987321','la punta',1,40),(35,'roman','riquelme','87987789','2665321026','mitre123',1,41),(36,'sofia','Aimar','54186237','1161970794','Los paraisos',1,45);
/*!40000 ALTER TABLE `socio` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuario`
--

DROP TABLE IF EXISTS `usuario`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuario` (
  `IdUsuario` int NOT NULL AUTO_INCREMENT,
  `Email` varchar(150) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `Roles` int NOT NULL,
  `Estado` tinyint(1) NOT NULL,
  `Avatar` varchar(100) DEFAULT NULL,
  `NombreUsuario` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`IdUsuario`),
  UNIQUE KEY `Email` (`Email`)
) ENGINE=InnoDB AUTO_INCREMENT=46 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuario`
--

LOCK TABLES `usuario` WRITE;
/*!40000 ALTER TABLE `usuario` DISABLE KEYS */;
INSERT INTO `usuario` VALUES (27,'JUAN123@GMAIL.COM','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',2,1,NULL,NULL),(28,'romina@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',1,1,NULL,NULL),(29,'pablo@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',1,1,NULL,NULL),(30,'tamara@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',1,1,NULL,NULL),(31,'tomas@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',1,1,NULL,NULL),(32,'for@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',1,1,NULL,NULL),(33,'joni@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',1,1,NULL,NULL),(34,'ke@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',1,1,NULL,NULL),(35,'brian@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',3,1,'/imagenes/avatares/8109516e-1b70-4e3c-9a3e-9471cf260277.jpg','brian@gmail.com'),(36,'yami@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',1,1,NULL,NULL),(37,'gabriela@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',3,1,NULL,NULL),(38,'fatima@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',3,1,NULL,NULL),(39,'fer@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',1,1,NULL,NULL),(40,'jose@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',1,1,NULL,NULL),(41,'lau@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',1,1,NULL,NULL),(42,'jose456@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',2,1,'/imagenes/avatares/3e958bac-f9f3-4aed-82b5-240191d8dcc7.jpg',NULL),(43,'romina123@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',2,1,'/imagenes/avatares/a3a34c6d-75c6-4ab3-8363-54312068b49d.jpg',NULL),(44,'pepe123@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',2,1,'/imagenes/avatares/dde88b52-691f-4623-9ce6-1bffe55faba2.jpg',NULL),(45,'victoria@gmail.com','yXqhyMX2Xr912eLwmvk+GBSvRNsU9gVYQdMqyqRq71U=',1,1,NULL,NULL);
/*!40000 ALTER TABLE `usuario` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'gimnasio'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-11-05 19:57:02
