CREATE DATABASE  IF NOT EXISTS `politutor2.0` /*!40100 DEFAULT CHARACTER SET utf8mb3 */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `politutor2.0`;
-- MySQL dump 10.13  Distrib 8.0.30, for Win64 (x86_64)
--
-- Host: 192.168.1.88    Database: politutor2.0
-- ------------------------------------------------------
-- Server version	8.0.31-0ubuntu0.20.04.1

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `active_tutoring`
--

DROP TABLE IF EXISTS `active_tutoring`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `active_tutoring` (
  `tutor` int NOT NULL,
  `exam` int DEFAULT NULL,
  `student` int NOT NULL,
  `is_OFA` tinyint NOT NULL DEFAULT '0',
  `start_date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `end_date` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`tutor`,`student`),
  KEY `exam` (`exam`),
  KEY `student` (`student`),
  CONSTRAINT `active_tutoring_ibfk_1` FOREIGN KEY (`tutor`) REFERENCES `tutor` (`tutor_code`) ON UPDATE CASCADE,
  CONSTRAINT `active_tutoring_ibfk_2` FOREIGN KEY (`exam`) REFERENCES `exam` (`code`) ON UPDATE CASCADE,
  CONSTRAINT `active_tutoring_ibfk_3` FOREIGN KEY (`student`) REFERENCES `enabled_student` (`student_code`) ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `active_tutoring`
--

LOCK TABLES `active_tutoring` WRITE;
/*!40000 ALTER TABLE `active_tutoring` DISABLE KEYS */;
/*!40000 ALTER TABLE `active_tutoring` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `course`
--

DROP TABLE IF EXISTS `course`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `course` (
  `name` varchar(300) NOT NULL,
  `school` varchar(300) NOT NULL,
  PRIMARY KEY (`name`),
  KEY `course_ibfk_1` (`school`),
  CONSTRAINT `course_ibfk_1` FOREIGN KEY (`school`) REFERENCES `school` (`name`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `course`
--

LOCK TABLES `course` WRITE;
/*!40000 ALTER TABLE `course` DISABLE KEYS */;
INSERT INTO `course` VALUES ('Ingegneria Aerospaziale','3I'),('Ingegneria Biomedica','3I'),('Ingegneria Chimica','3I'),('Ingegneria dei Materiali e delle Nanotecnologie','3I'),('Ingegneria dell\'Automazione','3I'),('Ingegneria della Produzione Industriale','3I'),('Ingegneria Elettrica','3I'),('Ingegneria Elettronica','3I'),('Ingegneria Energetica','3I'),('Ingegneria Fisica','3I'),('Ingegneria Gestionale','3I'),('Ingegneria Informatica','3I'),('Ingegneria Matematica','3I'),('Ingegneria Meccanica','3I'),('Ingegneria Civile','ICAT'),('Ingegneria Civile per La Mitigazione del Rischio','ICAT'),('Ingegneria per l\'Ambiente e il Territorio','ICAT');
/*!40000 ALTER TABLE `course` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `enabled_student`
--

DROP TABLE IF EXISTS `enabled_student`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `enabled_student` (
  `student_code` int NOT NULL,
  PRIMARY KEY (`student_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `enabled_student`
--

LOCK TABLES `enabled_student` WRITE;
/*!40000 ALTER TABLE `enabled_student` DISABLE KEYS */;
INSERT INTO `enabled_student` VALUES (111111),(222222),(938354);
/*!40000 ALTER TABLE `enabled_student` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `exam`
--

DROP TABLE IF EXISTS `exam`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `exam` (
  `code` int NOT NULL,
  `course` varchar(300) NOT NULL,
  `name` varchar(300) NOT NULL,
  `year` varchar(2) NOT NULL,
  PRIMARY KEY (`code`,`course`),
  KEY `exam_ibfk_1` (`course`),
  CONSTRAINT `exam_ibfk_1` FOREIGN KEY (`course`) REFERENCES `course` (`name`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `exam`
--

LOCK TABLES `exam` WRITE;
/*!40000 ALTER TABLE `exam` DISABLE KEYS */;
INSERT INTO `exam` VALUES (52400,'Ingegneria per l\'Ambiente e il Territorio','EQUAZIONI DIFFERENZIALI ORDINARIE','Y2'),(52522,'Ingegneria per l\'Ambiente e il Territorio','ECONOMIA AMBIENTALE','Y1'),(53385,'Ingegneria Civile per La Mitigazione del Rischio','CHIMICA A','Y1'),(53386,'Ingegneria Civile per La Mitigazione del Rischio','INFORMATICA','Y1'),(53387,'Ingegneria Civile per La Mitigazione del Rischio','GEOLOGIA APPLICATA 1','Y1'),(53391,'Ingegneria Civile per La Mitigazione del Rischio','MATERIALI PER LE STRUTTURE','Y1'),(53392,'Ingegneria Civile per La Mitigazione del Rischio','METODI DI ANALISI DI VULNERABILITÀ, RISCHIO E RESILIENZA','Y1'),(54080,'Ingegneria Civile per La Mitigazione del Rischio','GIS E GEOSTATISTICA','Y2'),(54086,'Ingegneria Civile per La Mitigazione del Rischio','EQUAZIONI DIFFERENZIALI ORDINARIE E MECCANICA RAZIONALE','Y2'),(54087,'Ingegneria Civile per La Mitigazione del Rischio','IDRAULICA','Y2'),(54096,'Ingegneria Civile per La Mitigazione del Rischio','TECNICHE DI MONITORAGGIO DEL DISSESTO IDROGEOLOGICO (WORKSHOP)','Y2'),(54177,'Ingegneria per l\'Ambiente e il Territorio','FISICA I','Y1'),(54180,'Ingegneria per l\'Ambiente e il Territorio','ECOLOGIA','Y2'),(54181,'Ingegneria per l\'Ambiente e il Territorio','MODELLISTICA E SIMULAZIONE','Y2'),(54804,'Ingegneria Civile per La Mitigazione del Rischio','ANALISI MATEMATICA E GEOMETRIA','Y1'),(55469,'Ingegneria per l\'Ambiente e il Territorio','FISICA II','Y2'),(55713,'Ingegneria Civile','MATHEMATICAL ANALYSIS II','Y1'),(55716,'Ingegneria Civile','MATHEMATICAL ANALYSIS I AND GEOMETRY','Y1'),(55717,'Ingegneria Civile','COMPUTER SCIENCE','Y1'),(55718,'Ingegneria Civile','CHEMISTRY','Y1'),(55721,'Ingegneria Civile','PHYSICS I AND PHYSICS IIA','Y1'),(55722,'Ingegneria Civile','CONSTRUCTION MATERIALS','Y1'),(55723,'Ingegneria Civile','ENGINEERING GEOLOGICAL SURVEY','Y1'),(56791,'Ingegneria Civile','DIFFERENTIAL EQUATIONS','Y2'),(56793,'Ingegneria Civile','SURVEYING AND DATA PROCESSING','Y2'),(56794,'Ingegneria Civile','RATIONAL MECHANICS','Y2'),(56795,'Ingegneria Civile','HYDRAULICS','Y2'),(56797,'Ingegneria Civile','STRUCTURAL MECHANICS','Y2'),(56798,'Ingegneria Civile','PROJECT MANAGEMENT: PRINCIPLES & TOOLS','Y2'),(57241,'Ingegneria Civile per La Mitigazione del Rischio','SCIENZA DELLE COSTRUZIONI','Y2'),(57246,'Ingegneria Civile per La Mitigazione del Rischio','FISICA SPERIMENTALE I E II','Y1'),(57295,'Ingegneria Civile per La Mitigazione del Rischio','DIRITTO DELL\'AMBIENTE E DEL TERRITORIO','Y2'),(82355,'Ingegneria per l\'Ambiente e il Territorio','GEOLOGIA AMBIENTALE','Y1'),(82358,'Ingegneria per l\'Ambiente e il Territorio','IDRAULICA','Y2'),(85639,'Ingegneria per l\'Ambiente e il Territorio','INGEGNERIA SANITARIA AMBIENTALE','Y2'),(97253,'Ingegneria per l\'Ambiente e il Territorio','CHIMICA A E CHIMICA AMBIENTALE','Y1'),(97254,'Ingegneria per l\'Ambiente e il Territorio','INFORMATICA','Y1'),(97280,'Ingegneria per l\'Ambiente e il Territorio','TRATTAMENTO DELLE OSSERVAZIONI','Y2'),(97290,'Ingegneria per l\'Ambiente e il Territorio','SCIENZA DELLE COSTRUZIONI I','Y2'),(97302,'Ingegneria Civile','FISICA TECNICA (ING. CIVILE)','Y2'),(97303,'Ingegneria per l\'Ambiente e il Territorio','ANALISI MATEMATICA E GEOMETRIA','Y1');
/*!40000 ALTER TABLE `exam` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `reservation`
--

DROP TABLE IF EXISTS `reservation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `reservation` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `tutor` int NOT NULL,
  `exam` int DEFAULT NULL,
  `student` int NOT NULL,
  `reservation_timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `is_processed` tinyint NOT NULL DEFAULT '0',
  `is_OFA` tinyint NOT NULL DEFAULT '0',
  PRIMARY KEY (`ID`),
  KEY `tutor` (`tutor`),
  KEY `exam` (`exam`),
  KEY `student` (`student`),
  CONSTRAINT `reservation_ibfk_1` FOREIGN KEY (`tutor`) REFERENCES `tutor` (`tutor_code`) ON UPDATE CASCADE,
  CONSTRAINT `reservation_ibfk_2` FOREIGN KEY (`exam`) REFERENCES `exam` (`code`) ON UPDATE CASCADE,
  CONSTRAINT `reservation_ibfk_3` FOREIGN KEY (`student`) REFERENCES `enabled_student` (`student_code`) ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `reservation`
--

LOCK TABLES `reservation` WRITE;
/*!40000 ALTER TABLE `reservation` DISABLE KEYS */;
INSERT INTO `reservation` VALUES (9,123322,NULL,111111,'2022-09-14 10:57:09',1,1),(11,123322,NULL,111111,'2022-09-14 14:22:36',1,1);
/*!40000 ALTER TABLE `reservation` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `school`
--

DROP TABLE IF EXISTS `school`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `school` (
  `name` varchar(300) NOT NULL,
  PRIMARY KEY (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `school`
--

LOCK TABLES `school` WRITE;
/*!40000 ALTER TABLE `school` DISABLE KEYS */;
INSERT INTO `school` VALUES ('3I'),('AUIC'),('Design'),('ICAT');
/*!40000 ALTER TABLE `school` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `telegram_user`
--

DROP TABLE IF EXISTS `telegram_user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `telegram_user` (
  `userID` int NOT NULL,
  `student_code` int NOT NULL,
  `lock_timestamp` timestamp NOT NULL DEFAULT '1970-01-01 11:00:10',
  PRIMARY KEY (`userID`),
  KEY `1_idx` (`student_code`),
  CONSTRAINT `telegram_user_ibfk_1` FOREIGN KEY (`student_code`) REFERENCES `enabled_student` (`student_code`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `telegram_user`
--

LOCK TABLES `telegram_user` WRITE;
/*!40000 ALTER TABLE `telegram_user` DISABLE KEYS */;
INSERT INTO `telegram_user` VALUES (1089557436,111111,'1970-01-01 11:00:10');
/*!40000 ALTER TABLE `telegram_user` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tutor`
--

DROP TABLE IF EXISTS `tutor`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tutor` (
  `tutor_code` int NOT NULL,
  `name` varchar(300) NOT NULL,
  `surname` varchar(300) NOT NULL,
  `course` varchar(300) NOT NULL,
  `OFA_available` tinyint NOT NULL DEFAULT '0',
  `ranking` int NOT NULL,
  PRIMARY KEY (`tutor_code`),
  KEY `course` (`course`),
  CONSTRAINT `tutor_ibfk_1` FOREIGN KEY (`course`) REFERENCES `course` (`name`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tutor`
--

LOCK TABLES `tutor` WRITE;
/*!40000 ALTER TABLE `tutor` DISABLE KEYS */;
INSERT INTO `tutor` VALUES (123123,'nome1','cognome1','Ingegneria Civile',0,1),(123322,'nome2','cognome2','Ingegneria dell\'Automazione',1,2),(123456,'nome3','cognome3','Ingegneria Chimica',0,3),(321321,'nome4','cognome4','Ingegneria Informatica',1,4);
/*!40000 ALTER TABLE `tutor` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tutor_to_exam`
--

DROP TABLE IF EXISTS `tutor_to_exam`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tutor_to_exam` (
  `tutor` int NOT NULL,
  `exam` int NOT NULL,
  `exam_professor` varchar(300) NOT NULL,
  `last_reservation` timestamp NOT NULL DEFAULT '1970-01-01 11:00:10',
  `available_tutorings` int NOT NULL DEFAULT '1',
  PRIMARY KEY (`tutor`,`exam`),
  KEY `exam` (`exam`),
  CONSTRAINT `tutor_to_exam_ibfk_1` FOREIGN KEY (`exam`) REFERENCES `exam` (`code`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `tutor_to_exam_ibfk_2` FOREIGN KEY (`tutor`) REFERENCES `tutor` (`tutor_code`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tutor_to_exam`
--

LOCK TABLES `tutor_to_exam` WRITE;
/*!40000 ALTER TABLE `tutor_to_exam` DISABLE KEYS */;
/*!40000 ALTER TABLE `tutor_to_exam` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'politutor2.0'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2022-10-30 17:11:21