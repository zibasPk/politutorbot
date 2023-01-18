import React, { useEffect, useState } from 'react';

import styles from './TutorManagement.module.css'
import configData from "../../config/config.json";
import validationConfig from "../../config/validation-config.json";
import examplePic from '../../assets/new-tutor-example.png';

import Papa from "papaparse";

import Form from 'react-bootstrap/Form';
import { Button } from 'react-bootstrap';
import InfoIconBis from '../utils/InfoIconBis';
import Collapse from '@mui/material/Collapse';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import FileUploadIcon from '@mui/icons-material/FileUpload';
import { allowedExtensions } from '../enabledStudents/EnabledStudents';


export default function AddTutor(props)
{
  const [courses, setCourses] = useState([]);
  const [formData, setFormData] = useState({
    TutorCode: null,
    Name: "",
    Surname: "",
    ExamCode: null,
    Course: "",
    Professor: null,
    Ranking: null,
    OfaAvailable: false,
    LastReservation: null,
    AvailableTutorings: null
  });
  const [tutorFileAlert, setFileAlertText] = useState("");
  const [tutoringFile, setTutoringFile] = useState(null);
  const [expanded, setExpanded] = useState(false);
  const [checkBoxState, setCheckBox] = useState(0);

  const refreshData = () =>
  {
    fetch(configData.botApiUrl + '/course', {
      method: 'GET',
      headers: {
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      }
    }).then(resp => resp.json())
      .then((courses) =>
      {
        courses.sort((a, b) => a.localeCompare(b));
        setCourses(courses);
        setFormData(params => ({
          ...params,
          Course: courses[0]
        }));
      })
  }

  useEffect(() =>
  {
    refreshData();
  }, [])

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
        setTutoringFile(null);
        setFileAlertText("File inserito non del formato .csv");
        return;
      }

      setTutoringFile(inputFile);
      setFileAlertText("");
    }
  }

  const sendTutorings = (tutorings, action) =>
  {
    fetch(configData.botApiUrl + '/tutor/' + action, {
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
          setFileAlertText("Errore nella richiesta: " + text)
          return;
        }
        // Hide alert after a positive response
        setFileAlertText("")
      })
  }

  const sendFile = (tutoringsFile, action) =>
  {
    // If user clicks the parse button without
    // a file 
    if (!tutoringsFile)
    {
      setFileAlertText("Inserire un file valido.");
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
      for (const tutoring of parsedData)
      {
        if (tutoring.OfaAvailable == "1")
          tutoring.OfaAvailable = true;
        else
          tutoring.OfaAvailable = false;

        alertMsg = validateTutoring(tutoring);
        if (alertMsg != null)
        {
          setFileAlertText("Errore nei dati per tutor " + tutoring.TutorCode + ": " + alertMsg);
          return false;
        }
      }

      if (alertMsg == null)
        sendTutorings(parsedData, action);
    };
    reader.readAsText(tutoringsFile);
  }


  const handleSubmit = (e) =>
  {
    let alertMsg = validateTutoring(formData);
    if (alertMsg == null)
    {
      sendTutorings([formData], "add");
      return;
    }
    setFileAlertText(alertMsg);
  }

  const handleFormChange = (e) =>
  {
    let value = e.target.value.trim();
    if (e.target.name === "OfaAvailable")
    {
      value = !checkBoxState;
      setCheckBox(value);
    }
    setFormData(
      {
        ...formData,

        // Trimming any whitespace
        [e.target.name]: value
      }
    )
  }

  const validateTutoring = (tutoring) =>
  {
    console.log(tutoring);
    if (tutoring.Name && tutoring.Name.length > validationConfig.maxNameLength)
      return 'La massima lunghezza per il Nome del tutor è ' + validationConfig.maxNameLength + " caratteri";

    if (tutoring.Surname && tutoring.Surname.length > validationConfig.maxSurnameLength)
      return 'La massima lunghezza per il Cognome del tutor è ' + validationConfig.maxSurnameLength + " caratteri";

    if (tutoring.Professor && tutoring.Professor.length > validationConfig.maxProfessorFullNameLength)
      return 'La massima lunghezza per il nome del Professore è ' + validationConfig.maxProfessorFullNameLength + " caratteri";

    if (!tutoring.TutorCode)
      return 'Codice matricola Tutor mancante';

    if (!tutoring.TutorCode.match(validationConfig.studentCodeRegex))
      return 'Codice matricola Tutor non valido';

    if (!tutoring.ExamCode)
      return 'Codice esame mancante';

    if (!tutoring.ExamCode.match(validationConfig.examCodeRegex))
      return 'Codice esame inserito non valido';

    if (!courses.includes(tutoring.Course))
      return 'Nome corso <' + tutoring.Course + '> non valido';

    if (isNaN(tutoring.Ranking))
      return 'Posizione in graduatoria <' + tutoring.Ranking + '> non valida';

    if (isNaN(tutoring.AvailableTutorings))
      return 'Diponibalità massima <' + tutoring.AvailableTutorings + '> non valida';

    if (typeof tutoring.OfaAvailable != "boolean")
      return 'Valore non valido per disponibilità OFA';

    return null;
  }

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

  return (
    <>
      <div className={styles.dropDownContent}>
        <h1>Aggiungi Tutor{icon}</h1>
        <Collapse in={expanded} timeout="auto" unmountOnExit>
          <div className={styles.addTutorContent}>
            <div className={styles.newTutorForm}>
              <Form>
                <Form.Group controlId="formTutorCode" className="mb-3">
                  <Form.Label>Matricola Tutor</Form.Label>
                  <Form.Control type="text" placeholder="Matr. Studente" name="TutorCode"
                    onChange={handleFormChange}
                  />
                </Form.Group>
                <Form.Group className="mb-3">
                  <Form.Label>Corso di studi</Form.Label>
                  <Form.Select name="Course" onChange={handleFormChange}>
                    {courses.map((course) => <option key={course}>{course}</option>)}
                  </Form.Select>
                </Form.Group>
                <Form.Group controlId="formTutorName" className="mb-3">
                  <Form.Label>Nome Tutor</Form.Label>
                  <Form.Control type="text" placeholder="Nome" name="Name"
                    onChange={handleFormChange}
                  />
                </Form.Group>
                <Form.Group controlId="formTutorSurname" className="mb-3">
                  <Form.Label>Cognome Tutor</Form.Label>
                  <Form.Control type="text" placeholder="Cognome" name="Surname"
                    onChange={handleFormChange}
                  />
                </Form.Group>
                <Form.Group controlId="formExamCode" className="mb-3">
                  <Form.Label>Esame</Form.Label>
                  <Form.Control type="text" placeholder="Codice Esame" name="ExamCode"
                    onChange={handleFormChange}
                  />
                </Form.Group>
                <Form.Group controlId="formExamCode" className="mb-3">
                  <Form.Label>Professore Avuto per l'esame</Form.Label>
                  <Form.Control type="text" placeholder="Nome Professore" name="Professor"
                    onChange={handleFormChange}
                  />
                </Form.Group>
                <Form.Group controlId="formExamCode" className="mb-3">
                  <Form.Label>Posizione in Graduatoria</Form.Label>
                  <Form.Control type="text" placeholder="Posizione" name="Ranking"
                    onChange={handleFormChange}
                  />
                </Form.Group>
                <Form.Group controlId="formExamCode" className="mb-3">
                  <Form.Label>Disponibilità massima</Form.Label>
                  <Form.Control type="text" placeholder="Posizione" name="AvailableTutorings"
                    onChange={handleFormChange}
                  />
                </Form.Group>
                <Form.Group className={styles.ofaSwitchGroup} controlId="formOFACheckbox">
                  <Form.Label>Disponibile per OFA</Form.Label>
                  <Form.Check type="switch" name="OfaAvailable"
                    onChange={handleFormChange}
                    className={styles.ofaSwitch}
                  />
                </Form.Group>
                <Button className={styles.addButton} variant="warning" type="button"
                  onClick={e => handleSubmit(e)}
                >
                  Aggiungi
                </Button>
                <div className={styles.tutorFileAlert}>{tutorFileAlert}</div>
              </Form>
            </div>
            <Form.Group controlId="formTutorFile" className="mb-3">

              <Form.Label>Carica File CSV Tutor</Form.Label>
              <InfoIconBis content={<>
                <div>Inserire un file cvs con righe come da figura:</div>
                <div><strong>Attenzione i nomi dell'intestazione devono essere come da figura **comprese maiuscole**</strong></div>
                <img src={examplePic}></img>
              </>} />

              <div className={styles.inputDiv}>
                <Form.Control type="file" onChange={(e) => handleFileChange(e)} />
                <FileUploadIcon className={styles.actionBox} onClick={() => sendFile(tutoringFile, "add")} />
              </div>
            </Form.Group>
          </div>
        </Collapse>
      </div>
    </>
  );
}