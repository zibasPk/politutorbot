
import styles from "./DataManagement.module.css";

import ExamData from "./ExamData";
import TutorData from "./TutorData";
import CourseData from "./CourseData";

function DataManagement()
{
  return (
    <>
      <div className={styles.content}>
        <TutorData />
        <ExamData />
        <CourseData />
      </div>
    </>
  );
}

export default DataManagement;