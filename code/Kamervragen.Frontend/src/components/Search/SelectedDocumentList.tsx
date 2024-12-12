import React from 'react';
import {Checkbox, makeStyles, tokens, Text, Card, CardHeader } from '@fluentui/react-components';
import { SelectedQAPair } from '../Search/QuestionAnswerList';
import Markdown from 'react-markdown';

const useClasses = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    width: '70%',
    backgroundColor: tokens.colorNeutralBackground1Hover,
    borderRadius: tokens.borderRadiusMedium,
    padding: tokens.spacingVerticalM,
    marginTop: tokens.spacingVerticalL,
    margin: 'auto',
  },
  scrollContainer: {
    flex: 1,
    heigth: '100%',
    display: 'flex',
    overflow: 'scroll',
    overflowX: 'hidden',
    flexDirection: 'column',
    '&::-webkit-scrollbar': {
        display: 'none'
    },
  },
  qaPair: {
    marginBottom: tokens.spacingVerticalS,
  },
  question: {
    fontWeight: tokens.fontWeightSemibold,
  },
  answer: {
    marginLeft: tokens.spacingHorizontalL,
  },
  card: {
    width: '100%',
    maxWidth: '100%',
    marginBottom: tokens.spacingVerticalS
  },
  qaContent: {
    // padding: tokens.spacingVerticalS,
  },
});

interface SelectedDocumentsListProps {
  selectedQAPairs: SelectedQAPair[];
  toggleQAPairSelection: (question: string) => void;
}

const SelectedDocumentsList: React.FC<SelectedDocumentsListProps> = ({ selectedQAPairs, toggleQAPairSelection }) => {
  const classes = useClasses();

  return (
    <div className={classes.scrollContainer}>
      <div className={classes.container}>
        {selectedQAPairs.map((qa) => (
          <Card key={qa.question} className={classes.card}>
            <CardHeader
              header={
                <div className={classes.question}>
                  <Checkbox
                    checked={true}
                    onChange={() => toggleQAPairSelection(qa.question)}
                    label={qa.question}
                    aria-label={`Select QA Pair: ${qa.question}`}
                  />
                </div>
              }
            />
            <div className={classes.qaContent}>
              {qa.answer && <Markdown className={classes.answer}>{qa.answer}</Markdown>}
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
};

export default SelectedDocumentsList;