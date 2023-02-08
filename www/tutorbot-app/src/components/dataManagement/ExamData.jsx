import React, {useState } from 'react';

import styles from './DataManagement.module.css'
import configData from "../../config/config.json";
import validationConfig from "../../config/validation-config.json";
import examplePic from "../../assets/new-exam-example.png";

import Papa from "papaparse";

import UploadForm from '../utils/UploadForm';
import Collapse from '@mui/material/Collapse';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import DeleteButton from '../utils/DeleteButton';

export default function ExamData()
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

  const parseExamFile = (file, alertSetter, sendFile) =>
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
        alertMsg = validateExam(exam);
        if (alertMsg != null)
        {
          alertSetter("Errore nei dati per l'esame <" + exam.Code + "> in corso <" + exam.Course + ">: " + alertMsg);
          return false;
        }
      }
      if (alertMsg == null)
        sendFile(parsedData);
      return;
    };
    reader.readAsText(file);

  }

  const validateExam = (exam) =>
  {
    if (!exam.Code)
      return 'Codice Esame mancante';

    if (!exam.Code.match(validationConfig.examCodeRegex))
      return 'Codice Esame inserito non valido';

    if (!exam.Course)
      return 'Corso mancante';

    if (!exam.Name)
      return 'Nome Esame mancante';

    if (!exam.Year)
      return 'Anno Esame mancante';

    if (!validationConfig.validYears.includes(exam.Year))
      return 'Anno Esame invalido';

    return null;
  }


  return (
    <>
      <div className={styles.dropDownContent}>
        <h1>Gestione Dati Esami{icon}</h1>
        <Collapse in={expanded} timeout="auto" unmountOnExit className={styles.examDataCont}>
          <UploadForm
            formText="Carica File CSV Esami da aggiungere"
            infoContent={
              <>
                <div>Inserire un file .csv con righe come da figura:</div>
                <div><strong>Attenzione i nomi dell'intestazione devono essere come da figura **comprese le maiuscole**</strong></div>
                <img src={examplePic}></img>
              </>}
            uploadEndPoint="/exam/add"
            parseData={(file, alertSetter, sendFile) => parseExamFile(file, alertSetter, sendFile)}
          />
          <div>Usa questa funzionalità per eliminare tutti i dati sugli Esami dal sistema</div>
          <DeleteButton 
          btnText="Reset dati Esami" 
          modalTitle='Eliminazione esami' 
          deleteEndpoint={configData.botApiUrl + '/exams/'}
          />
        </Collapse>
      </div>
    </>
  );
}
