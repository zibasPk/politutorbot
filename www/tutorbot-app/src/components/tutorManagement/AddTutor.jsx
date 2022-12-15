import React, { useEffect, useState } from 'react';

import styles from './TutorManagement.module.css'
import configData from "../../config/config.json";
import pic from '../../assets/excel-pic.png';

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
  const [expanded, setExpanded] = useState(true);
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
        setFormData({ Course: courses[0] });
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
          setFileAlertText(text)
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
      const csv = Papa.parse(target.result, { header: true, skipEmptyLines: true });
      const parsedData = csv?.data;
      parsedData.forEach((tutoring) =>
      {
        if (tutoring.OfaAvailable == "1")
          tutoring.OfaAvailable = true;
        else
          tutoring.OfaAvailable = false;
      }

      )
      sendTutorings(parsedData, action);
    };
    reader.readAsText(tutoringsFile);
  }


  const handleSubmit = (e) =>
  {
    sendTutorings([formData], "add")
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
                <Form.Group className="mb-3" controlId="formOFACheckbox">
                  <Form.Check label="disponibile per OFA" type="switch" name="OfaAvailable"
                    onChange={handleFormChange}
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
                <img src={pic}></img>
              </>} />
              {/* <InfoIcon text={<img src={pic}></img>} /> */}
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