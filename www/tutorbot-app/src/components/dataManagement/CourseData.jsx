import React, { useState } from 'react';

import styles from './DataManagement.module.css'
import configData from "../../config/config.json";
import validationConfig from "../../config/validation-config.json";
import examplePic from "../../assets/new-course-example.png";

import Papa from "papaparse";

import UploadForm from '../utils/UploadForm';
import Collapse from '@mui/material/Collapse';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';
import { makeCall } from '../../MakeCall';

export default function CourseData()
{
  const [expanded, setExpanded] = useState(false);

  const handleExpandClick = () =>
  {
    setExpanded(!expanded);
  };

  const icon = !expanded ? <ExpandMoreIcon
    expand={expanded.toString()}
    onClick={handleExpandClick}
    aria-expanded={expanded}
    aria-label="show more"
    fontSize='none'
    className={styles.btnExpand}
  /> : <ExpandLessIcon
    expand={expanded.toString()}
    onClick={handleExpandClick}
    aria-expanded={expanded}
    aria-label="show more"
    fontSize='none'
    className={styles.btnExpand}
  />;

  const parseCourseFile = (file, alertSetter, sendFile) =>
  {

    // If user clicks the parse button without
    // a file 
    if (!file)
    {
      alertSetter("Inserire un file valido.");
      return;
    }

    // Initialize a reader which allows user
    // to read any file or blob.
    const reader = new FileReader();


    // Event listener on reader when the file
    // loads, we parse it and send the data.
    reader.onload = async ({ target }) =>
    {
      let alertMsg = null;

      const csv = Papa.parse(target.result, { header: true, skipEmptyLines: true });
      const parsedData = csv?.data;
      for (const exam of parsedData)
      {
        alertMsg = validateCourse(exam);
        if (alertMsg != null)
        {
          alertSetter("Errore nei dati per corso " + exam.Course + ": " + alertMsg);
          return false;
        }
      }
      if (alertMsg == null)
        sendFile(parsedData);
      return;
    };
    reader.readAsText(file);

  }

  const validateCourse = (course) =>
  {
    if (!course.Name)
      return 'Nome Corso mancante';

    if (!course.School)
      return 'Sigla Scuola mancante';

    if (!validationConfig.validSchools.includes(course.School))
      return 'Sigla Scuola invalida, può essere solo: ' + validationConfig.validSchools.toString();

    return null;
  }


  return (
    <>
      <div className={styles.dropDownContent}>
        <h1>Gestione Dati Corsi di Studi{icon}</h1>
        <Collapse in={expanded} timeout="auto" unmountOnExit className={styles.courseDataCont}>
          <UploadForm
            formText="Carica File CSV Corsi da aggiungere"
            infoContent={
              <>
                <div>Inserire un file .csv con righe come da figura:</div>
                <div><strong>Attenzione i nomi dell'intestazione devono essere come da figura **comprese maiuscole**</strong></div>
                <img src={examplePic} alt="immagine mancante"></img>
              </>}
            uploadEndPoint="/course/add"
            parseData={(file, alertSetter, sendFile) => parseCourseFile(file, alertSetter, sendFile)}
          />
          <DeleteCourses />
        </Collapse>
      </div>
    </>
  );
}

function DeleteCourses()
{
  const [show, setShow] = useState(false);
  const [modalText, setText] = useState("");
  const [textClass, setTextClass] = useState(styles.deletionAlert);
  const [showButton, setShowButton] = useState(true);

  const handleShow = () =>
  {
    setText("Attenzione! Questa azione é irreversibile sei sicuro di voler continuare?");
    setTextClass(styles.deletionAlert);
    setShow(true);
    setShowButton(true);
  };

  const handleConfirm = async () =>
  {
    let status = { code: 0 };
    let result = await makeCall({ url: configData.botApiUrl + '/courses/', method: 'DELETE', hasAuth: true, status: status });

    if (status.code !== 200) {
      if (result === "") {
        return;
      }
      setText(result);
      return;
    }

    setText("Eliminazione avvenuta con successo");
    setTextClass(styles.successAlert);
    setShowButton(false);
  }

  return (
    <>
      <div>Usa questa funzionalità per eliminare tutti i dati sui Corsi di Studio dal sistema</div>
      <Button className={styles.btnDeleteTutors} variant="danger" onClick={handleShow}>
        Reset dati Corsi di Studio
      </Button>
      <Modal show={show} onHide={() => setShow(false)} dialogClassName={styles.deleteTutorModal}>
        <Modal.Header closeButton>
          <Modal.Title>Eliminazione datu Corsi di Studio</Modal.Title>
        </Modal.Header>
        <Modal.Body className={textClass}>{modalText}</Modal.Body>
        {
          showButton ? <Modal.Footer>
            <Button variant="danger" onClick={handleConfirm}>
              Conferma
            </Button>
          </Modal.Footer>
            :
            <></>
        }
      </Modal>
    </>);
}
