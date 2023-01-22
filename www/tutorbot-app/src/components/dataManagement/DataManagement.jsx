
import styles from "./DataManagement.module.css";

import ExamData from "./ExamData";
import TutorData from "./TutorData";
import CourseData from "./CourseData";
import DataHistory from "./DataHistory";

function DataManagement()
{
  return (
    <>
      <div className={styles.content}>
        <div className={styles.pageDescription}>
          <p>
          <strong>Attenzione!</strong> L'eliminazione dei dati sui tutor e tutoraggi impedirà la visualizzazione e modifica di prenotazioni e tutoraggi che li riguardano.
          </p>
          <p>
            Le informazioni eliminate e gli storici riguardanti gli anni passati saranno ancora disponibili e reperibili da questa pagina.
          </p>
          <p>
            <strong>Attenzione! </strong>Il reset dei dati è raccomandato solo in caso di pulizia del database a fine anno accademico.
          </p>
        </div>
        <TutorData />
        <ExamData />
        <CourseData />
        <DataHistory/>
      </div>
    </>
  );
}

export default DataManagement;