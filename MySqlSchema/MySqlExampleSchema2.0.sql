CREATE DATABASE  IF NOT EXISTS `politutor2.0` /*!40100 DEFAULT CHARACTER SET utf8 */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `politutor2.0`;
-- MySQL dump 10.13  Distrib 8.0.28, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: politutor2.0
-- ------------------------------------------------------
-- Server version	8.0.28

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
  `ID` int NOT NULL AUTO_INCREMENT,
  `tutor` int NOT NULL,
  `exam` int DEFAULT NULL,
  `student` int NOT NULL,
  `is_OFA` tinyint NOT NULL DEFAULT '0',
  `start_date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `end_date` timestamp NULL DEFAULT NULL,
  `duration` int DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `exam` (`exam`),
  CONSTRAINT `active_tutoring_ibfk_2` FOREIGN KEY (`exam`) REFERENCES `exam` (`code`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `active_tutoring`
--

LOCK TABLES `active_tutoring` WRITE;
/*!40000 ALTER TABLE `active_tutoring` DISABLE KEYS */;
INSERT INTO `active_tutoring` VALUES (1,321321,52522,111111,0,'2022-12-11 14:36:39','2022-12-11 17:02:28',3),(2,321321,52522,111111,0,'2022-12-11 14:36:39',NULL,NULL),(3,321321,52522,111111,1,'2022-12-11 14:36:39','2022-12-15 12:18:50',11),(4,321321,52522,111111,0,'2022-12-11 14:36:39',NULL,NULL),(5,123322,NULL,111111,1,'2022-12-15 12:09:41',NULL,NULL),(6,123322,NULL,111111,1,'2022-12-15 12:19:26','2022-12-15 12:21:02',123),(7,123322,NULL,123123,1,'2022-12-22 12:46:02',NULL,NULL),(8,123333,NULL,123123,1,'2022-12-22 12:46:02',NULL,NULL),(15,321321,NULL,123123,1,'2022-12-22 14:06:30',NULL,NULL),(17,999999,52522,222222,0,'2023-01-31 10:28:12','2023-01-31 10:38:22',123);
/*!40000 ALTER TABLE `active_tutoring` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `check_duplicate_active_tutorings` BEFORE INSERT ON `active_tutoring` FOR EACH ROW BEGIN
	DECLARE duplicate int;
	SET duplicate = (SELECT COUNT(*) FROM active_tutoring
    WHERE tutor = NEW.tutor AND (exam = NEW.exam OR (exam IS NULL AND NEW.exam IS NULL)) 
    AND student = NEW.student AND end_date IS NULL);	
    IF duplicate > 0 THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Duplicate entry on active_tutoring table';
	END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `save_active_tutoring_history` AFTER INSERT ON `active_tutoring` FOR EACH ROW BEGIN
INSERT INTO active_tutoring_history (ID,tutor_code,tutor_name,tutor_surname,student_code,exam_code,is_OFA,start_date,end_date,duration) 
	SELECT a.ID,t.tutor_code,t.name,t.surname,a.student,a.exam,a.is_OFA,a.start_date,a.end_date,a.duration 
    FROM tutor as t join active_tutoring as a ON t.tutor_code = a.tutor WHERE a.ID =  NEW.ID;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `update_tutor_hours` AFTER INSERT ON `active_tutoring` FOR EACH ROW BEGIN
    IF NEW.duration IS NOT NULL THEN
        UPDATE tutor SET hours_done = hours_done + NEW.duration 
        WHERE NEW.tutor = tutor_code;
    END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `update_active_tutoring_history` AFTER UPDATE ON `active_tutoring` FOR EACH ROW BEGIN
	UPDATE active_tutoring_history SET duration = NEW.duration, end_date = NEW.end_date 
    WHERE active_tutoring_history.ID = NEW.ID;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `active_tutoring_history`
--

DROP TABLE IF EXISTS `active_tutoring_history`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `active_tutoring_history` (
  `ID` int NOT NULL,
  `tutor_code` varchar(300) DEFAULT NULL,
  `tutor_name` varchar(300) DEFAULT NULL,
  `tutor_surname` varchar(300) DEFAULT NULL,
  `student_code` varchar(45) DEFAULT NULL,
  `exam_code` varchar(300) DEFAULT NULL,
  `is_OFA` varchar(300) DEFAULT NULL,
  `start_date` varchar(300) DEFAULT NULL,
  `end_date` varchar(300) DEFAULT NULL,
  `duration` varchar(300) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `active_tutoring_history`
--

LOCK TABLES `active_tutoring_history` WRITE;
/*!40000 ALTER TABLE `active_tutoring_history` DISABLE KEYS */;
INSERT INTO `active_tutoring_history` VALUES (17,'999999','giulio','bartolomei','222222','52522','0','2023-01-31 11:28:12','2023-01-31 11:38:22','123');
/*!40000 ALTER TABLE `active_tutoring_history` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `auth_token`
--

DROP TABLE IF EXISTS `auth_token`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `auth_token` (
  `token` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`token`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `auth_token`
--

LOCK TABLES `auth_token` WRITE;
/*!40000 ALTER TABLE `auth_token` DISABLE KEYS */;
INSERT INTO `auth_token` VALUES ('1aD/zrwCCJbVUa6vVPjBaFMfKmYOf8+hUJnCT6/Z13w=','2023-02-03 18:33:17'),('6OTAJvNvMaTrEH6Z5DzkFWF+G4X/+YEPsdRnbFxqJDs=','2023-02-03 17:55:57'),('75be5GO0pYnK/HGM1tqX3ehl9I8jftySiycG2793f+s=','2023-02-03 18:19:51'),('7T6XOq9s2sKlUA2asI9hjegZWdXqO+4UnYgqFKsS9NA=','2023-02-03 17:00:52'),('9d1M6Dn8lGs317sO77bn+KJnLUDaWO31seHontJZbhQ=','2023-02-03 16:52:38'),('9rsD9/fpCAsj7Hwn6zdbugZwcikxItmCWQPXHLQ1fRA=','2023-02-03 17:06:06'),('aPBoTz0M+H465GPCCwpr2+uNyShQkmdPCFC8TFLvmW8=','2023-02-09 11:42:35'),('aQPdmOIXAdFHaAwmsaaXN83ViKmhMNhkwtEDMTb5e4s=','2023-02-03 16:55:26'),('c1os+6857Obrf5Mi8DiRAgLwtFKq0b03yVIYQqm/Zds=','2023-02-03 18:30:35'),('cUzuAKDo2XZOZIPpGol8wJa490MNzD9YdMbFyzawtPQ=','2023-02-03 17:37:21'),('dMjSYNhYsiKUC0Lsx4IuGWXzA72Jm+r0x2sYS1vmyjU=','2023-02-03 18:06:50'),('Drq1lPTTRtMPccKL2CUBecZyT7Vh1u/vRbZzfJBHi5c=','2023-02-07 17:14:59'),('evr5LtNV6PeVIcC5Mu6FPDqFgwgg6eHYQkTCD9GvBtw=','2023-02-03 17:34:28'),('fE2R1nrr7lGW8mkcFAUewyT/mtoTGeXQFSp5tiGRYmk=','2023-02-07 17:13:00'),('fVmYDu2DsnAB7hcKi+zJyaAZVGzrcYBw80sabYvLh+I=','2023-02-03 17:37:26'),('gcdY4+UnqAmEpc/AnFeIGtNxMg8FAIZRR5UTujcN/NM=','2023-02-03 19:08:10'),('gCrF5n9m87/9Feqe0Ay//sqjjftyLKSOe/WsmxPG+NQ=','2023-02-03 17:50:44'),('gz70odKKLcmBSbaPMUmDdolR1Hz76l1e+tujHhRqy7M=','2023-02-03 17:00:58'),('HoXHr+IlGf7WgNKiLpTDZwqgzEdrPbpk089EGmBNOjw=','2023-02-03 18:31:34'),('HUWAP8jjCv1D5srOCytmLLY/3hz3OMp3ZO03HP9JLkQ=','2023-02-03 18:18:52'),('IWXywequFstY7io9FnceyamHmFVOeD+sHLNjbk6/Nmw=','2023-02-03 20:01:12'),('Jah8eHyAvL+1/J2ThJc+HQh2QJv2TMTGUubavUC8YtA=','2023-02-07 17:12:33'),('Jo5Iq2/SnobE7Y9nIwn6EIXZfB8v3tpG37PipJBXx0I=','2023-02-03 17:00:59'),('kJ9q1HtL9QkTvKyMXsof2KL8gcoLi8FxTwbMKKb/i1s=','2023-02-03 17:00:58'),('kr93muxEcy3cKPQR8E59dfcvGXFbbdcuBU4ErfkG1f8=','2023-02-07 17:15:33'),('L5rvxSIa4Ajbr5kgFjKjRRO8dC93XinCnubNMb5fXx8=','2023-02-07 17:10:17'),('lNqVSaYH6lhOM8k1/pgr2LqulQ3aos7VOE+8sp3pkfc=','2023-02-03 16:50:20'),('lTjaQqvWWie5UcAfatzh24m4xI6gINDVbVWUMLEgGpQ=','2023-02-03 17:59:17'),('lxcqAUujSj4nnjWUyXdNv4vVwBZPJ+EojQ3YiWydZTo=','2023-02-03 17:03:33'),('MIw8i/7vCO7qYiP4IA8x9nP6UPwFWW0W3NY1FWGESnY=','2023-02-03 17:57:33'),('NeBdyXaZY7V4IePrRd12GCle8s2CG+xy8xX3fiTgt3U=','2023-02-03 17:26:13'),('NVyY/rgjya/s7N7i4jk+9KCbbcH16vtHJcsUiylDXxk=','2023-02-03 17:05:42'),('ODfWJOAbpHfi9EebGrO/VWOzXODOO6Hmkl2QhVX13V0=','2023-02-03 18:20:18'),('Q22OgFbTH2Bf1oBiGULXkMzuN0NKtpddW4709ZvVW1E=','2023-02-03 16:49:28'),('Rja22D5Lx6jmCxYIT9/UhpfSsbd7yrs+Mmlw21mwi+g=','2023-02-03 16:54:18'),('rpRJf5n8ApvtAoC2y3HI6r1/vEGlBMK4XbSCmuW+KCY=','2023-02-03 18:03:28'),('rud/rgnF/7t12PCAdHdyslXhy2n3Ipc2ioFyolYKoic=','2023-02-03 19:58:33'),('S0xahVWgS0m/tq50/NXi1DFQC4H2/BaAI3eDegKrH78=','2023-02-07 17:13:15'),('s1jM8IUN6wMGEeb4KN6nT+ZsGbIeE9A/unhhgoxkyfA=','2023-02-03 17:38:02'),('sOPAcoW7lhFpj9LmpHqeJ0X2/68C7B+kPvpbrr7ojqQ=','2023-02-07 17:11:43'),('TL3Xp9tp/HIHnU5ZFTo++AgyWwzpoDCd/ec0xnd2sdE=','2023-02-07 17:15:26'),('uz1Nd1tp7XanN9NDDArkSsJkWz9pBA5X6vKSFR3RWnc=','2023-02-07 17:09:26'),('uzL7ISQ5jGw5t982OeYvI1y9spd7BH/U8w/Y3vCYCA0=','2023-02-07 17:13:56'),('vqJgkY9+1bEkDnouOgKzkW+V0mUnIc5qXDE4/uNYccU=','2023-02-03 19:34:25'),('vuGdHMxStEZ9jtuvIKHNeJoMQrwkqkUSPh0zbX0Wl3A=','2023-02-03 19:30:48'),('WiDASiR+G34wkr/G5/z3O6k39ao86gBDX3N2la/bKjU=','2023-02-03 18:57:34'),('WjeeeXCKGmD2vgWspYGFykSjFqtWgTNG9bemzb/PWGI=','2023-02-03 18:36:48'),('Wrod4kkH8ssD7BxJZxwtjMCIlQkRfv4+H5yXod+GUgI=','2023-02-03 17:00:54'),('x+b816aU2XzMtaCMxt8KTCY7ZaZ4YhUP63hMwr3FoLI=','2023-02-03 17:48:46'),('xR7PFkAHq2/A/anprOTd4jD5pOelm9JD19745rwhiUo=','2023-02-03 19:28:50'),('Yihlprnxfv0LCTID009yls2gkw5ffKf8sVzcVDs3IwY=','2023-02-03 17:30:57'),('YNrMHlL/b9nh3ffxf0YrtxJofdHQlMzWAKxVpX4z/2s=','2023-02-03 17:36:44');
/*!40000 ALTER TABLE `auth_token` ENABLE KEYS */;
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
INSERT INTO `enabled_student` VALUES (123123),(222222),(444444),(938354),(999900),(999998),(999999);
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
  KEY `exam` (`exam`),
  CONSTRAINT `reservation_ibfk_2` FOREIGN KEY (`exam`) REFERENCES `exam` (`code`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `reservation`
--

LOCK TABLES `reservation` WRITE;
/*!40000 ALTER TABLE `reservation` DISABLE KEYS */;
INSERT INTO `reservation` VALUES (5,321321,52522,111111,'2022-12-09 13:42:44',1,0),(9,123322,NULL,111111,'2022-09-14 10:57:09',1,1),(11,123322,NULL,111111,'2022-09-14 14:22:36',1,1),(12,999999,52522,222222,'2023-01-13 15:40:35',1,0),(13,999999,52522,222222,'2023-01-13 17:38:20',1,0),(15,123123,52522,444444,'2023-01-31 10:04:30',1,0);
/*!40000 ALTER TABLE `reservation` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `save_reservation_history` AFTER INSERT ON `reservation` FOR EACH ROW BEGIN
INSERT INTO reservation_history (ID,tutor_code,tutor_name,tutor_surname,exam_code,student_code,reservation_date) 
	SELECT r.ID,t.tutor_code,t.name,t.surname,r.exam,r.student,r.reservation_timestamp 
    FROM tutor as t join reservation as r ON t.tutor_code = r.tutor WHERE r.ID =  NEW.ID;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `reservation_history`
--

DROP TABLE IF EXISTS `reservation_history`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `reservation_history` (
  `ID` int NOT NULL,
  `tutor_code` varchar(300) DEFAULT NULL,
  `tutor_name` varchar(300) DEFAULT NULL,
  `tutor_surname` varchar(300) DEFAULT NULL,
  `exam_code` varchar(300) DEFAULT NULL,
  `student_code` varchar(300) DEFAULT NULL,
  `reservation_date` varchar(300) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `reservation_history`
--

LOCK TABLES `reservation_history` WRITE;
/*!40000 ALTER TABLE `reservation_history` DISABLE KEYS */;
INSERT INTO `reservation_history` VALUES (14,'123123','nome1','cognome1','52522','444444','2023-01-31 11:04:30'),(15,'123123','nome1','cognome1','52522','444444','2023-01-31 11:04:30');
/*!40000 ALTER TABLE `reservation_history` ENABLE KEYS */;
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
INSERT INTO `telegram_user` VALUES (107050697,444444,'1970-01-01 11:00:10'),(1089557436,222222,'1970-01-01 11:00:10');
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
  `contract_state` int NOT NULL DEFAULT '0',
  `hours_done` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`tutor_code`),
  UNIQUE KEY `ranking_UNIQUE` (`ranking`),
  KEY `course` (`course`),
  CONSTRAINT `tutor_ibfk_1` FOREIGN KEY (`course`) REFERENCES `course` (`name`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tutor`
--

LOCK TABLES `tutor` WRITE;
/*!40000 ALTER TABLE `tutor` DISABLE KEYS */;
INSERT INTO `tutor` VALUES (111111,'Gabriele','Musel','Ingegneria Biomedica',1,21,0,0),(123333,'mario','marioni','Ingegneria Biomedica',1,123,0,0),(222222,'Franco','Juvara','Ingegneria per l\'Ambiente e il Territorio',0,22,0,0),(333333,'Mario','Craxi','Ingegneria Matematica',1,24,0,0),(444444,'Giovanni','Garibaldi','Ingegneria Informatica',1,23,0,0),(555555,'Gabriele','Petra','Ingegneria Biomedica',0,31,0,0),(666666,'Franco','Trullio','Ingegneria per l\'Ambiente e il Territorio',0,32,0,0),(777777,'Giovanni','Parsivaldi','Ingegneria Informatica',0,33,0,0),(888888,'Paola','Felice','Ingegneria Matematica',1,34,0,0),(938333,'mario','franchini','Ingegneria Aerospaziale',1,987,0,0),(999999,'giulio','bartolomei','Ingegneria per l\'Ambiente e il Territorio',0,999,0,0);
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
INSERT INTO `tutor_to_exam` VALUES (111111,52400,'nome cognome','1970-01-01 11:00:10',1),(123333,54096,'franco franconi','1970-01-01 11:00:10',1),(222222,52522,'nome cognome','1970-01-01 11:00:10',1),(333333,82355,'Francesco Sforza','1970-01-01 11:00:10',1),(444444,97280,'Francesco Sforza','1970-01-01 11:00:10',1),(555555,52400,'nome cognome','1970-01-01 11:00:10',1),(666666,54086,'nome cognome','1970-01-01 11:00:10',1),(777777,97280,'Francesco Sforza','1970-01-01 11:00:10',1),(888888,82355,'Francesco Sforza','1970-01-01 11:00:10',1),(938333,52400,'francesco lucciola','1970-01-01 11:00:10',1),(999999,52522,'mario franceschini','1970-01-01 11:00:10',1);
/*!40000 ALTER TABLE `tutor_to_exam` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `save_tutoring_history` AFTER INSERT ON `tutor_to_exam` FOR EACH ROW BEGIN
	INSERT INTO tutoring_history (tutor_code,tutor_name,tutor_surname,tutor_course,tutor_ranking,tutor_OFA_availability,exam_code,exam_professor,available_tutorings)
	SELECT tutor_code,name,surname,course,ranking,OFA_available,exam,exam_professor,available_tutorings 
    FROM tutor join tutor_to_exam ON tutor_code = tutor WHERE tutor_code =  NEW.tutor AND exam = NEW.exam;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `tutoring_history`
--

DROP TABLE IF EXISTS `tutoring_history`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tutoring_history` (
  `id` int NOT NULL AUTO_INCREMENT,
  `tutor_code` varchar(300) DEFAULT NULL,
  `tutor_name` varchar(300) DEFAULT NULL,
  `tutor_surname` varchar(300) DEFAULT NULL,
  `tutor_course` varchar(300) DEFAULT NULL,
  `tutor_ranking` varchar(300) DEFAULT NULL,
  `tutor_OFA_availability` varchar(300) DEFAULT NULL,
  `exam_code` varchar(300) DEFAULT NULL,
  `exam_professor` varchar(300) DEFAULT NULL,
  `available_tutorings` varchar(300) DEFAULT NULL,
  `date` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tutoring_history`
--

LOCK TABLES `tutoring_history` WRITE;
/*!40000 ALTER TABLE `tutoring_history` DISABLE KEYS */;
INSERT INTO `tutoring_history` VALUES (1,'938333','mario','franchini','Ingegneria Aerospaziale','987','1','52400','francesco lucciola','1','2023-01-31 10:15:58'),(2,'111111','Gabriele','Musel','Ingegneria Biomedica','21','1','52400','nome cognome','1','2023-02-07 16:38:51'),(3,'222222','Franco','Juvara','Ingegneria per l\'Ambiente e il Territorio','22','0','52522','nome cognome','1','2023-02-07 16:38:51'),(4,'444444','Giovanni','Garibaldi','Ingegneria Informatica','23','1','97280','Francesco Sforza','1','2023-02-07 16:38:51'),(5,'333333','Mario','Craxi','Ingegneria Matematica','24','1','82355','Francesco Sforza','1','2023-02-07 16:38:51'),(6,'111111','Gabriele','Musel','Ingegneria Biomedica','21','1','52400','nome cognome','1','2023-02-07 17:02:11'),(7,'222222','Franco','Juvara','Ingegneria per l\'Ambiente e il Territorio','22','0','52522','nome cognome','1','2023-02-07 17:02:11'),(8,'444444','Giovanni','Garibaldi','Ingegneria Informatica','23','1','97280','Francesco Sforza','1','2023-02-07 17:02:11'),(9,'333333','Mario','Craxi','Ingegneria Matematica','24','1','82355','Francesco Sforza','1','2023-02-07 17:02:11'),(10,'555555','Gabriele','Petra','Ingegneria Biomedica','31','0','52400','nome cognome','1','2023-02-07 17:45:46'),(11,'666666','Franco','Trullio','Ingegneria per l\'Ambiente e il Territorio','32','0','54086','nome cognome','1','2023-02-07 17:45:46'),(12,'777777','Giovanni','Parsivaldi','Ingegneria Informatica','33','0','97280','Francesco Sforza','1','2023-02-07 17:45:46'),(13,'888888','Paola','Felice','Ingegneria Matematica','34','1','82355','Francesco Sforza','1','2023-02-07 17:45:46');
/*!40000 ALTER TABLE `tutoring_history` ENABLE KEYS */;
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

-- Dump completed on 2023-02-09 14:12:53
