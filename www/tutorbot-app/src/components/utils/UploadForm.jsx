import React, { useState } from "react";
import styles from "./UploadForm.module.css"
import configData from "../../config/config.json";

import Form from 'react-bootstrap/Form';

import InfoIconBis from '../utils/InfoIconBis';
import FileUploadIcon from '@mui/icons-material/FileUpload';
import CheckIcon from '@mui/icons-material/Check';
import Grow from '@mui/material/Grow';
import { makeCall } from "../../MakeCall";
import { Spinner } from "react-bootstrap";

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
  const [isPending, setIsPending] = useState(false);
  const [success, setSuccess] = useState(false);


  const handleFileChange = (e) =>
  {
    setSuccess(false);
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


  const sendFile = async (tutorings) =>
  {
    setIsPending(true);
    let status = { code: 0 }
    let result = await makeCall({ url: configData.botApiUrl + props.uploadEndPoint, method: "POST", hasAuth: true, status: status, body: JSON.stringify(tutorings) });

    setIsPending(false);
    if (status.code !== 200)
    {
      setFileAlertText("Errore nella richiesta: " + result);
      return;
    }
    // Hide alert after a positive response
    setFileAlertClass(styles.successText);
    setFileAlertText("");
    // set success to true to show the success icon
    setSuccess(true);
    if (props.callBack)
      props.callBack();
  }


  const successIcon = <CheckIcon className='successIcon' />;

  return (
    <>
      <Form.Group controlId="formTutorFile" className="mb-3">
        <Form.Label>{props.formText}</Form.Label>
        <InfoIconBis content={props.infoContent} />
        {isPending && <Spinner animation="border" className={styles.pendingCircle} />}
        <div className={alertClass}>{fileAlert}</div>
        <div className={styles.inputDiv}>
          <Form.Control type="file" onChange={(e) => handleFileChange(e)} />
          <FileUploadIcon className={styles.actionBox} onClick={() => parseFile(uploadedFile)} />
          <Grow in={success} timeout={500}>{successIcon}</Grow>
        </div>
      </Form.Group>

    </>
  );
}

export default UploadForm;