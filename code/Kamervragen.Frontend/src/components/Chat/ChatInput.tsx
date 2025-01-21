import { makeStyles, tokens, Button, Input, Toolbar } from '@fluentui/react-components';
import { BroomRegular, Document10016Regular, Question16Filled, Send24Regular } from '@fluentui/react-icons';

const useClasses = makeStyles({
    container: {
        flexDirection: 'column',
        display: 'flex',
        height: '100px',
        width: '70%',
        margin: 'auto',
        alignItems: 'center',
        gap: tokens.spacingHorizontalS,
        justifyContent: 'space-between',
        paddingTop: tokens.spacingVerticalL,
        paddingBottom: tokens.spacingVerticalL,
        marginBottom: tokens.spacingVerticalL,
    },
    input: {
        flexGrow: 1,
        height: '60px',
        borderRadius: tokens.borderRadiusMedium,
        backgroundColor: tokens.colorNeutralBackground2
    },
    button: {
        height: '60px'
    },
    toolbarContainer: {
        marginBottom: tokens.spacingVerticalS,
        marginTop: tokens.spacingVerticalS

    },
    toolbar: {
        marginBottom: tokens.spacingVerticalS,
      },
      inputSection: {
        display: 'flex',
        alignItems: 'center',
        width: '100%',
        gap: tokens.spacingHorizontalM,
        marginBottom: tokens.spacingVerticalS,
      },
      toggleButtonActive: {
        backgroundColor: tokens.colorBrandBackground,
        color: tokens.colorNeutralForegroundOnBrand,
      },
      toggleButtonInactive: {
        backgroundColor: tokens.colorTransparentBackground,
        color: tokens.colorNeutralForeground1,
      },
});

type chatInputType = {
    value: string,
    setValue: (value: string ) => void,
    onSubmit: (message: string) => void,
    clearChat: () => void,
    includeQA: boolean;
    includeDocs: boolean;
    toggleIncludeQA: () => void;
    toggleIncludeDocs: () => void;
}

export function ChatInput({ value, setValue, onSubmit,clearChat, includeQA,
    includeDocs,
    toggleIncludeQA,
    toggleIncludeDocs }: chatInputType) {
    const classes = useClasses();
  
    const handleKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
        if (event.key === 'Enter') {
          handleSubmit();
        }
      };
    

    const handleSubmit = () => {
        onSubmit(value);
      };

    return (
        <div className={classes.container}>
      <Toolbar className={classes.toolbar}>
      <Button
          icon={<Document10016Regular />}
          title="Gebruik mijn documenten"
          aria-label="Gebruik mijn documenten"
          onClick={toggleIncludeDocs}
          appearance={includeDocs ? 'primary' : 'transparent'}
          className={includeDocs ? classes.toggleButtonActive : classes.toggleButtonInactive}
        />
        <Button
          icon={<Question16Filled />}
          title="Gebruik vraagstukken"
          aria-label="Gebruik vraagstukken"
          onClick={toggleIncludeQA}
          appearance={includeQA ? 'primary' : 'transparent'}
          className={includeQA ? classes.toggleButtonActive : classes.toggleButtonInactive}
        />
      </Toolbar>
      <div className={classes.inputSection}>
        <Button
          className={classes.button}
          icon={<BroomRegular />}
          onClick={clearChat}
          aria-label="Clear session"
          role="button"
          tabIndex={0}
        />
      <Input
        onKeyDown={handleKeyDown}
        className={classes.input}
        size="large"
        value={value}
        onChange={(_e, data) => setValue(data.value)}
        placeholder="Type your message..."
      />
      <Button
        className={classes.button}
        onClick={handleSubmit}
        size="large"
        icon={<Send24Regular />}
        aria-label="Send message"
      />
      </div>
    </div>
    );
};