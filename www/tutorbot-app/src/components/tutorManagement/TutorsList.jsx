import React from 'react';
import styles from './TutorManagement.module.css'
import Collapse from '@mui/material/Collapse';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import Table from '../utils/Table';


export default function TutorsList(props) {
  const [expanded, setExpanded] = React.useState(true);

  const handleExpandClick = () => {
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
        <h1>Lista Tutor{icon}</h1>
        <Collapse in={expanded} timeout="auto" unmountOnExit>
          <div className={styles.tutorListContent}>
            <Table headers={props.headers} content={props.tutorList} hasChecks={false}
              modalProps={{
                modalTitle: "Cambia stato tutor selezionati"
              }} />
          </div>
        </Collapse>
      </div>
    </>
  );
}