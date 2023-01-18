
import { useState } from "react";

import styles from "./TutorManagement.module.css";
import configData from "../../config/config.json";

import { Button } from 'react-bootstrap';

function TutoringListModal(props)
{
  const [alert, setAlert] = useState("");


  const removeTutorings = () =>
  {
    console.log(props.selectedContent);
    let toDelete = props.selectedContent.map((tutoring) =>
    {
      return { "TutorCode": tutoring.TutorCode, "ExamCode": tutoring.ExamCode }
    });

    fetch(configData.botApiUrl + '/tutoring/', {
      method: 'DELETE',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Basic ' + btoa(configData.authCredentials),
      },
      body: JSON.stringify(toDelete)
    }).then(resp =>
    {
      if (!resp.ok)
        return resp.text();
      props.onModalEvent();
    })
      .then((text) =>
      {
        if (text !== undefined)
        {
          setAlert(text);
          return;
        }
      })

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