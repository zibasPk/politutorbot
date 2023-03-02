import React, { useState } from 'react';

import styles from "./ActiveTutorings.module.css";
import configData from "../../config/config.json";
import validationConfig from "../../config/validation-config.json";
import examplePic from "../../assets/activate-tutoring-example.png"
import Papa from "papaparse";


import Form from 'react-bootstrap/Form';
import { Button } from 'react-bootstrap';
import { makeCall } from '../../MakeCall';
import UploadForm from '../utils/UploadForm';
import { Spinner } from "react-bootstrap";


function ManualActivation(props) {
  const [checkBoxState, setCheckBox] = useState(0);
  const [alertText, setAlert] = useState("");
  const [isPending, setIsPending] = useState(false)


  const handleSubmit = () => {
    let alertMsg = validateTutoring(formData);
    if (alertMsg != null) {
      setAlert("Errore nei dati inseriti: " + alertMsg);
      return;
    }
    sendTutorings([formData]);
  };

  const [formData, setFormData] = useState({
    IsOFA: false,
    TutorCode: null,
    StudentCode: null,
    ExamCode: null
  });

  const handleFormChange = (e) => {
    let value = e.target.value.trim();
    if (e.target.name === "IsOFA") {
      value = !checkBoxState;
      setCheckBox(value);
      if (value) {
        formData.ExamCode = null;
      }
    }
    setFormData(
      {
        ...formData,

        // Trimming any whitespace
        [e.target.name]: value
      }
    )
    setAlert("");
  }



  const parseTutoringsFile = (file, alertSetter, sendFile) => {
    // If user clicks the parse button without
    // a file we show a error
    if (!file) {
      alertSetter("Nessun file selezionato");
      return;
    };

    // Initialize a reader which allows user
    // to read any file or blob.
    const reader = new FileReader();

    // Event listener on reader when the file
    // loads, we parse it and send the data.
    reader.onload = async ({ target }) => {
      let alertMsg = null;

      const csv = Papa.parse(target.result, { header: true, skipEmptyLines: true });
      const parsedData = csv?.data;

      for (const tutoring of parsedData) {
        if (tutoring.IsOFA == "1")
          tutoring.IsOFA = true;
        else
          tutoring.IsOFA = false;

        if (tutoring.ExamCode === "") {
          tutoring.ExamCode = null;
        }

        alertMsg = validateTutoring(tutoring);
        if (alertMsg != null) {
          alertSetter("Errore nei dati per (tutor: "
            + tutoring.TutorCode + " studente: " + tutoring.StudentCode + "): " + alertMsg);
          return;
        }
      }
      if (alertMsg == null)
        sendFile(parsedData);
    };
    reader.readAsText(file);
  }

  const sendTutorings = async (tutorings) => {
    setIsPending(true);
    let status = { code: 0 }
    let result = await makeCall({ url: configData.botApiUrl + '/tutoring/start', method: 'POST', hasAuth: true, body: JSON.stringify(tutorings), status: status });
    setIsPending(false);

    if (status.code !== 200) {
      setAlert("Errore nella richiesta: " + result);
      return;
    }
    props.onChange();

    // Hide alert after a positive response
    setAlert("");
  }

  const validateTutoring = (tutoring) => {
    if (tutoring.IsOFA && tutoring.ExamCode)
      return "Un tutoraggio non può essere per OFA ed avere un codice esame";

    if (!tutoring.TutorCode)
      return 'Codice matricola Tutor mancante';

    if (!tutoring.TutorCode.match(validationConfig.studentCodeRegex))
      return 'Codice matricola Tutor non valido';

    if (!tutoring.IsOFA && !tutoring.ExamCode)
      return 'Codice esame mancante';

    if (!tutoring.IsOFA && !tutoring.ExamCode.match(validationConfig.examCodeRegex))
      return 'Codice esame inserito non valido';

    if (!tutoring.StudentCode)
      return 'Codice matricola studente mancante';

    if (!tutoring.StudentCode.match(validationConfig.studentCodeRegex))
      return 'Codice matricola studente non valido';

    return null;
  }

  const disabled = formData.IsOFA;

  return (
    <>
      <div>Attiva un nuovo tutoraggio:</div>
      <div className="contentWithBg">
        <div className={styles.activateForm} >
          <Form.Check label="per OFA" type="switch" name="IsOFA"
            onChange={handleFormChange}
            className={styles.ofaSwitch}
          />
          <Form.Control type="text" placeholder="Matr. Tutor" name="TutorCode"
            onChange={handleFormChange}
            className={styles.activateInput}
          />
          <Form.Control type="text" placeholder="Matr. Studente" name="StudentCode"
            onChange={handleFormChange}
            className={styles.activateInput}
          />
          <Form.Control type="text" placeholder="Codice Esame" name="ExamCode"
            onChange={handleFormChange}
            className={styles.activateInput}
            disabled={disabled}
          />
          <Button variant="warning" type="button"
            onClick={e => handleSubmit(e)}
            className={styles.activateInput}
          >
            Attiva
          </Button>
          {isPending && <Spinner animation="border" className={styles.pendingCircle} />}
        </div>
        <div className={styles.AlertText}>{alertText}</div>
        <div className={styles.inputDiv}>
          <UploadForm
            formText="Carica File CSV con i tutoraggi da attivare"
            infoContent=
            {
              <>
                <div>Inserire un file cvs con righe come da figura:</div>
                <div><strong>Attenzione i nomi dell'intestazione devono essere come da figura **comprese le maiuscole**</strong></div>
                <img src={examplePic} alt="immagine mancante"></img>
              </>
            }
            uploadEndPoint="/tutoring/start"
            parseData={(file, alertSetter, sendFile) => parseTutoringsFile(file, alertSetter, sendFile)}
            callBack={() => props.onChange()}
          />
        </div>
      </div>
    </>);
}

export default ManualActivation;