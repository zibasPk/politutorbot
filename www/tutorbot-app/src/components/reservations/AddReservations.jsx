import React, { useEffect, useState } from 'react';

import styles from './Reservations.module.css';
import configData from "../../config/config.json";
import validationConfig from "../../config/validation-config.json";
import examplePic from '../../assets/new-tutor-example.png';

import Papa from "papaparse";

import Collapse from '@mui/material/Collapse';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import CheckIcon from '@mui/icons-material/Check';
import UploadForm from '../utils/UploadForm';



export default function AddReservations(props)
{
  const [expanded, setExpanded] = useState(false);
  // useEffect(() =>
  // {
  //   refreshData();
  // }, [])

  // const sendTutorings = async (tutorings, action) =>
  // {
  //   setIsPending(true);
  //   let status = { code: 0 };
  //   let result = await makeCall({url: configData.botApiUrl + '/tutor/' + action, method: 'POST', hasAuth: true, status: status, body: JSON.stringify(tutorings)});
  //   setIsPending(false);
  //   if (status.code !== 200)
  //   {
  //     setFileAlertText("Errore nella richiesta: " + result);
  //     return false;
  //   }
  //   // Hide alert after a positive response
  //   setFileAlertText("");
  //   props.onChange();
  //   return true;
  // }

  const parseResFile = (file, alertSetter, sendFile) =>
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
      console.log(parsedData);
      if (alertMsg == null)
        sendFile(parsedData);
      return;
    };
    reader.readAsText(file);
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

  const successIcon = <CheckIcon className='successIcon'/>;

  return (
    <>
      <div className={styles.dropDownContent}>
        <h1>Aggiungi Richieste{icon}</h1>
        <Collapse in={expanded} timeout="auto" unmountOnExit>
          <div className={styles.addTutorContent + " contentWithBg"}>
            <UploadForm
              formText="Carica File CSV Richieste"
              infoContent={
                <>
                  <div>Inserire un file .csv con righe come da figura:</div>
                  <div><strong>Attenzione i nomi dell'intestazione devono essere come da figura **comprese le maiuscole**</strong></div>
                  <img src={examplePic}></img>
                </>}
              uploadEndPoint="/reservations/add"
              parseData={(file, alertSetter, sendFile) => parseResFile(file, alertSetter, sendFile)}
              callBack={() => props.onChange()}
            />
          </div>
        </Collapse>
      </div>
    </>
  );
}