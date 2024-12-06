import React, { useState } from 'react';
import { SearchBox } from '@fluentui/react/lib/SearchBox';
import { Field, makeStyles, ProgressBar, tokens } from '@fluentui/react-components';
import SearchResultCard from './SearchResultCard';
import { SelectedQAPair } from './QuestionAnswerList';
import { useSearch } from '../../hooks/useSearch';

const useClasses = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    width: '70%',
    borderRadius: tokens.borderRadiusMedium,
    padding: tokens.spacingVerticalM,
    boxShadow: tokens.shadow2,
    marginTop: tokens.spacingVerticalL
  },
  searchResults:
  {
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
  searchbox:
  {
    paddingTop:tokens.spacingVerticalM,
    paddingBottom:tokens.spacingVerticalM
  },
  header: {
      flexGrow: 1,
      display: 'flex',
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'space-between',
  },
  title: {
      fontSize: tokens.fontSizeBase300,
      color: tokens.colorNeutralForeground1,
      whiteSpace: 'nowrap',
      overflow: 'hidden',
      width: '200px',
      textOverflow: 'ellipsis',
  },
  selected: {
      backgroundColor: tokens.colorBrandBackground2Hover
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
messageContainer: {
    width: '70%',
    margin: 'auto',
    height: 'calc(100vh - 60px)',
},
thinkingContainer: {
  width: '50%',
  backgroundColor: tokens.colorNeutralBackground1Pressed,
  borderRadius: tokens.borderRadiusXLarge,
  padding: tokens.spacingHorizontalL,
}
});

interface SearchInterfaceProps {
  chatId: string | undefined;
  onQAPairsSelected: (selectedQAPairs: SelectedQAPair[]) => void;
}

const SearchInterface: React.FC<SearchInterfaceProps> = ({ chatId, onQAPairsSelected }) => {
  const { documents, isLoading, searchDocuments, getAnswers } = useSearch(chatId);
  const [allSelectedQAPairs, setAllSelectedQAPairs] = useState<SelectedQAPair[]>([]);
  const classes = useClasses();

  const handleSearch = (query: string) => {
    searchDocuments(query);
  };

  const updateSelectedQAPairs = (documentId: string, selectedPairs: SelectedQAPair[]) => {
    setAllSelectedQAPairs((prev) => {
      const otherPairs = prev.filter((pair) => pair.documentId !== documentId);
      const updatedPairs = [...otherPairs, ...selectedPairs];
      onQAPairsSelected(updatedPairs);
      return updatedPairs;
    });
  };

  return (
    <div className={classes.scrollContainer}>
      <div className={classes.messageContainer}>
        <div className={classes.searchbox}>
          <SearchBox placeholder="Search" onSearch={handleSearch} />
        </div>
        {isLoading && (
          <div className={classes.thinkingContainer}>
            <Field validationMessage="Searching..." validationState="none">
              <ProgressBar />
            </Field>
          </div>
        )}
        {!isLoading && (
          <div className={classes.searchResults}>
            {documents.map((item) => (
              <SearchResultCard
                key={item.id}
                document={item}
                onQAPairsSelected={updateSelectedQAPairs}
                getAnswers={getAnswers}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default SearchInterface;