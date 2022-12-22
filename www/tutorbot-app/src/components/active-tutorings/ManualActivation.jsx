import React, { Component, useState } from 'react';

import styles from "./ActiveTutorings.module.css";
import configData from "../../config/config.json";
import validationConfig from "../../config/validation-config.json";
import Papa from "papaparse";


import InfoIcon from '../utils/InfoIcon';

import Form from 'react-bootstrap/Form';
import { Button } from 'react-bootstrap';
import FileUploadIcon from '@mui/icons-material/FileUpload';
import { allowedExtensions } from '../enabledStudents/EnabledStudents';


function ManualActivation(props)
{
  const [checkBoxState, setCheckBox] = useState(0);
  const [file, setFile] = useState(null);
  const [alertText, setAlert] = useState("");


  const handleSubmit = () => { 
    let alertMsg = validateTutoring(formData);
    if(alertMsg != null ) {
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

  const handleFormChange = (e) =>
  {
    let value = e.target.value.trim();
    if (e.target.name === "IsOFA")
    {
      value = !checkBoxState;
      setCheckBox(value);
      if (value)
      {
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

  const handleFileChange = (e) =>
  {
    if (e.target.files.length)
    {
      const inputFile = e.target.files[0]

      // Check the file extensions, if it not
      // included in the allowed extensions
      // we show the error
      const fileExtension = inputFile?.type.split("/")[1];
      if (!allowedExtensions.includes(fileExtension))
      {
        setFile(null);
        setAlert("File inserito non del formato .csv")
        return;
      }

      setFile(inputFile);
      setAlert("");
    }

  }

  const sendFile = (tutorings) =>
  {
    // If user clicks the parse button without
    // a file we show a error
    if (!tutorings)
    {
      setAlert("Inserire un file valido.");
      return;
    };

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
      
      for (const tutoring of parsedData) {
        if (tutoring.IsOFA == "1")
          tutoring.IsOFA = true;
        else
          tutoring.IsOFA = false;

        if (tutoring.ExamCode === "")
        {
          tutoring.ExamCode = null;
        }

        alertMsg = validateTutoring(tutoring);
        if (alertMsg != null)
        {
          setAlert("Errore nei dati per (tutor: "
            + tutoring.TutorCode + " studente: " + tutoring.StudentCode + "): " + alertMsg);
          return;
        }
      }
      
      if (alertMsg == null)
        sendTutorings(parsedData);
    };
    reader.readAsText(tutorings);
  }

  const sendTutorings = (tutorings) =>
  {
    fetch(configData.botApiUrl + '/tutoring/start', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      },
      body: JSON.stringify(tutorings)
    }).then(resp =>
    {
      if (!resp.ok)
        return resp.text();
      props.onChange();
    })
      .then((text) =>
      {
        if (text !== undefined)
        {
          setAlert("Errore nella richiesta: " +text);
          return;
        }
        // Hide alert after a positive response
        setAlert("")
      })
  }

  const validateTutoring = (tutoring) =>
  {
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
      </div>
      <div className={styles.AlertText}>{alertText}</div>
      <Form.Group controlId="formFileEnable" className="mb-3">
        <Form.Label>Carica File CSV</Form.Label>
        <InfoIcon text="Caricare un file CVS contente un elenco (**in colonna**) di codici matricola da abilitare." />
        <div className={styles.inputDiv}>
          <Form.Control type="file" onChange={(e) => handleFileChange(e)} />
          <FileUploadIcon className={styles.actionBox}
            onClick={() => sendFile(file)} />
        </div>
      </Form.Group>

    </>);
}

export default ManualActivation;