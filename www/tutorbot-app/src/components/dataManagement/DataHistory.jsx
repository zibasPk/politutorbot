import React, { useEffect, useState } from 'react';

import styles from './DataManagement.module.css'
import configData from "../../config/config.json";
import validationConfig from "../../config/validation-config.json";
import examplePic from "../../assets/new-exam-example.png";

import Papa from "papaparse";

import { jsonToCSV } from 'react-papaparse'

import UploadForm from '../utils/UploadForm';
import Collapse from '@mui/material/Collapse';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import FileDownloadIcon from '@mui/icons-material/FileDownload';
import Form from 'react-bootstrap/Form';
import Button from 'react-bootstrap/Button';
import Modal from 'react-bootstrap/Modal';

export default function DataHistory()
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

  const exportCsv = async (title, csvFields, dataArray) =>
  {

    const csv = jsonToCSV(
      {
        fields: csvFields,
        data: dataArray
      }, { delimiter: ";" });

    var blob = new Blob([csv], { type: 'text/plain' });
    const element = document.createElement("a");
    element.href = URL.createObjectURL(blob);
    element.download = title;
    document.body.appendChild(element);
    element.click();
  }


  return (
    <>
      <div className={styles.dropDownContent}>
        <h1>Download Dati Storici{icon}</h1>
        <Collapse in={expanded} timeout="auto" unmountOnExit className={styles.historyDataCont}>
          <p><Form.Label className={styles.csvDownloadLable}>Download storico Tutoraggi Svolti</Form.Label>
            <FileDownloadIcon className={styles.btnDownloadCvs} onClick={() => exportCsv()} /></p>

          <p><Form.Label className={styles.csvDownloadLable}>Download storico Prenotazioni</Form.Label>
            <FileDownloadIcon className={styles.btnDownloadCvs} onClick={() => exportCsv()} /></p>

        </Collapse>
      </div>
    </>
  );
}
