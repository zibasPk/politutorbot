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
  `tutor` int NOT NULL,
  `exam` varchar(300) NOT NULL,
  `student` int NOT NULL,
  PRIMARY KEY (`tutor`,`exam`,`student`),
  KEY `exam` (`exam`),
  KEY `student` (`student`),
  CONSTRAINT `active_tutoring_ibfk_1` FOREIGN KEY (`tutor`) REFERENCES `tutor` (`student_code`),
  CONSTRAINT `active_tutoring_ibfk_2` FOREIGN KEY (`exam`) REFERENCES `exam` (`code`),
  CONSTRAINT `active_tutoring_ibfk_3` FOREIGN KEY (`student`) REFERENCES `enabled_student` (`student_code`)
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
INSERT INTO `course` VALUES ('Ingegneria Aerospaziale','3I'),('Ingegneria Biomedica','3I'),('Ingegneria Chimica','3I'),('Ingegneria dei Materiali e delle Nanotecnologie','3I'),('Ingegneria dell\'Automazione','3I'),('Ingegneria della Produzione Industriale','3I'),('Ingegneria Elettrica','3I'),('Ingegneria Elettronica','3I'),('Ingegneria Energetica','3I'),('Ingegneria Fisica','3I'),('Ingegneria Gestionale','3I'),('Ingegneria Informatica','3I'),('Ingegneria Matematica','3I'),('Ingegneria Meccanica','3I'),('Ingegneria Ambientale','ICAT'),('Ingegneria Civile','ICAT'),('Ingegneria Territoriale','ICAT');
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
  `code` varchar(300) NOT NULL,
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
INSERT INTO `exam` VALUES ('123','Ingegneria Civile','analisi 1','Y1'),('123','Ingegneria Informatica','analisi 1','Y1'),('141','Ingegneria Civile','fondamenti di internet e reti','Y1'),('14124','Ingegneria Civile','Onde elettromagnetiche e mezzi trasmissivi','Y1'),('313','Ingegneria Civile','fondamenti di automazione','Y1'),('321','Ingegneria Civile','analisi 2','Y2'),('444','Ingegneria Civile','fondamenti di informatica','Y1');
/*!40000 ALTER TABLE `exam` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `reservation`
--

DROP TABLE IF EXISTS `reservation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `reservation` (
  `tutor` int NOT NULL,
  `exam` varchar(300) NOT NULL,
  `student` int NOT NULL,
  `reservation_timestamp` varchar(300) NOT NULL DEFAULT 'NOW()',
  PRIMARY KEY (`tutor`,`exam`,`student`),
  KEY `exam` (`exam`),
  KEY `student` (`student`),
  CONSTRAINT `reservation_ibfk_1` FOREIGN KEY (`tutor`) REFERENCES `tutor` (`student_code`),
  CONSTRAINT `reservation_ibfk_2` FOREIGN KEY (`exam`) REFERENCES `exam` (`code`),
  CONSTRAINT `reservation_ibfk_3` FOREIGN KEY (`student`) REFERENCES `enabled_student` (`student_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `reservation`
--

LOCK TABLES `reservation` WRITE;
/*!40000 ALTER TABLE `reservation` DISABLE KEYS */;
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
  `lock_timestamp` timestamp NOT NULL,
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
/*!40000 ALTER TABLE `telegram_user` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tutor`
--

DROP TABLE IF EXISTS `tutor`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tutor` (
  `student_code` int NOT NULL,
  `name` varchar(300) NOT NULL,
  `surname` varchar(300) NOT NULL,
  `course` varchar(300) NOT NULL,
  `OFA_available` tinyint NOT NULL DEFAULT '0',
  PRIMARY KEY (`student_code`),
  KEY `course` (`course`),
  CONSTRAINT `tutor_ibfk_1` FOREIGN KEY (`course`) REFERENCES `course` (`name`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tutor`
--

LOCK TABLES `tutor` WRITE;
/*!40000 ALTER TABLE `tutor` DISABLE KEYS */;
INSERT INTO `tutor` VALUES (123123,'nome','cognome','Ingegneria Civile',0),(123322,'nome','cognome','Ingegneria dell\'Automazione',0),(123456,'nome','cognome','Ingegneria Chimica',0),(321321,'nome','cognome','Ingegneria Informatica',0);
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
  `exam` varchar(300) NOT NULL,
  `exam_professor` varchar(300) NOT NULL,
  `last_reservation` timestamp NOT NULL DEFAULT '0000-00-00 00:00:00',
  `available_reservations` int NOT NULL DEFAULT '1',
  PRIMARY KEY (`tutor`,`exam`),
  KEY `exam` (`exam`),
  CONSTRAINT `tutor_to_exam_ibfk_1` FOREIGN KEY (`tutor`) REFERENCES `tutor` (`student_code`),
  CONSTRAINT `tutor_to_exam_ibfk_2` FOREIGN KEY (`exam`) REFERENCES `exam` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tutor_to_exam`
--

LOCK TABLES `tutor_to_exam` WRITE;
/*!40000 ALTER TABLE `tutor_to_exam` DISABLE KEYS */;
INSERT INTO `tutor_to_exam` VALUES (123123,'123','nome cognome','0000-00-00 00:00:00',1),(123123,'321','nome cognome','0000-00-00 00:00:00',1),(123456,'123','nome cognome','0000-00-00 00:00:00',1),(321321,'123','nome cognome','0000-00-00 00:00:00',1);
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

-- Dump completed on 2022-08-31 14:34:03
