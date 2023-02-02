
import { useState } from "react";

import styles from "./TutorManagement.module.css";
import configData from "../../config/config.json";

import { Button } from 'react-bootstrap';
import { makeCall } from "../../MakeCall";

function TutoringListModal(props)
{
  const [alert, setAlert] = useState("");

  const removeTutorings = async () =>
  {
    let toDelete = props.selectedContent.map((tutoring) =>
    {
      return { "TutorCode": tutoring.TutorCode, "ExamCode": tutoring.ExamCode }
    });

    let status = { code: 0 };
    let result = await makeCall(configData.botApiUrl + '/tutoring/', 'DELETE', 'application/json', true, 
    JSON.stringify(toDelete), status);

    if (status.code !== 200)
    {
      setAlert(result);
      return;
    }
    props.onModalEvent();
  }

  const headerRow = Object.keys(props.contentHeaders).map((key, i) =>
  {
    const cell = <th key={i}>{props.contentHeaders[key]}</th>
    return cell;
  });

  const contentRow = (row) => Object.keys(props.contentHeaders).map((key, i) =>
  {
    return <td key={i}>{row[key]}</td>;
  });
  
  if (props.selectedContent.length !== 0) 
  {
    return (
      <>
        <div className={styles.alertText}>{alert}</div>
        <table>
          <thead>
            <tr>
              {headerRow}
            </tr>
          </thead>
          <tbody>
            {props.selectedContent.map((tutoring, i) => <tr key={i}>
              {contentRow(tutoring)}
            </tr>)}
          </tbody>
        </table>
        <div>
          <Button className={styles.confirmStateChangeBtn} variant="danger" onClick={() => removeTutorings()}>
            Conferma Eliminazione
          </Button>
        </div>
      </>
    );
  }
  else
    return (<div>Nessun Tutoraggio Selezionato</div>);
}

export default TutoringListModal;