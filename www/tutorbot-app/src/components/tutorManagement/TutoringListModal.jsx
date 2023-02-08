
import { useState } from "react";

import styles from "./TutorManagement.module.css";
import configData from "../../config/config.json";

import { Button } from 'react-bootstrap';
import { makeCall } from "../../MakeCall";
import { Spinner } from "react-bootstrap";

function TutoringListModal(props)
{
  const [alert, setAlert] = useState("");
  const [isPending, setIsPending] = useState(false);

  const removeTutorings = async () =>
  {
    let toDelete = props.selectedContent.map((tutoring) =>
    {
      return { "TutorCode": tutoring.TutorCode, "ExamCode": tutoring.ExamCode }
    });

    setIsPending(true);
    let status = { code: 0 };
    let result = await makeCall({ url: configData.botApiUrl + '/tutoring/', method: "DELETE", hasAuth: true, status: status, body: JSON.stringify(toDelete) });
    setIsPending(false);

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
          <div className={styles.confirmStateChangeDiv}>
            {isPending && <div className={styles.pendingCircleModal}><Spinner animation="border" /></div>}
            <Button className={styles.confirmStateChangeBtn} variant="danger" onClick={() => removeTutorings()}>
              Conferma Eliminazione
            </Button>
          </div>
        </div>
      </>
    );
  }
  else
    return (<div>Nessun Tutoraggio Selezionato</div>);
}

export default TutoringListModal;