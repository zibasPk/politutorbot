import React, { useState } from "react";
import styles from "./UploadForm.module.css"
import configData from "../../config/config.json";

import Form from 'react-bootstrap/Form';

import InfoIconBis from '../utils/InfoIconBis';
import FileUploadIcon from '@mui/icons-material/FileUpload';

export const allowedExtensions = ["csv", "vnd.ms-excel"];

/**
 * Generalized JSX Component for uploading a .csv file 
 * @param {object} props
 * @param {string} props.formText text of form lable
 * @param {React.Component} props.infoContent content of info popup
 * @param {string} props.uploadEndPoint string of end point url
 * @param {(file, alertSetter) => string} props.parseData function to parse uploaded file 
 * @param {() => void} props.callBack call back function to be called after upload
 */
function UploadForm(props)
{
  const [fileAlert, setFileAlertText] = useState("");
  const [alertClass, setFileAlertClass] = useState(styles.alertText);
  const [uploadedFile, setUploadedFile] = useState(null);


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
        setUploadedFile(null);
        setFileAlertText("File inserito non del formato .csv");
        return;
      }

      setUploadedFile(inputFile);
      setFileAlertText("");
      setFileAlertClass(styles.alertText);
    }
  }

  const parseFile = (tutoringsFile) =>
  {
    setFileAlertClass(styles.alertText);
    props.parseData(tutoringsFile,
      (text) => setFileAlertText(text),
      (file) => sendFile(file));
  }


  const sendFile = (tutorings) =>
  {
    fetch(configData.botApiUrl + props.uploadEndPoint, {
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
      if (props.callBack)
        props.callBack()
    })
      .then((text) =>
      {
        if (text !== undefined)
        {
          setFileAlertText("Errore nella richiesta: " + text)
          return;
        }
        // Hide alert after a positive response
        setFileAlertClass(styles.successText);
        setFileAlertText("Caricamento avvenuto con successo");
      })
  }



  return (
    <>
      <Form.Group controlId="formTutorFile" className="mb-3">
        <Form.Label>{props.formText}</Form.Label>
        <InfoIconBis content={props.infoContent} />
        <div className={alertClass}>{fileAlert}</div>
        <div className={styles.inputDiv}>
          <Form.Control type="file" onChange={(e) => handleFileChange(e)} />
          <FileUploadIcon className={styles.actionBox} onClick={() => parseFile(uploadedFile)} />
        </div>
      </Form.Group>
    </>
  );
}

export default UploadForm;