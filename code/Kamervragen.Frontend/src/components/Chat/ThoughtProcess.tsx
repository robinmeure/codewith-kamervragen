import { Stack } from "@fluentui/react";
import ReactJsonView from '@microlink/react-json-view'
import { makeStyles, tokens } from '@fluentui/react-components';
import { Thoughts } from "../../models/ChatMessage";

interface Props {
    thoughts: Thoughts[];
}

const useClasses = makeStyles({
    thoughtProcess: {
        fontFamily: 'source-code-pro, Menlo, Monaco, Consolas, "Courier New", monospace',
        wordWrap: 'break-word',
        paddingTop: '0.75em',
        paddingBottom: '0.75em',
      },
      tList: {
        padding: '1.25em 1.25em 0 1.25em',
        display: 'inline-block',
        backgroundColor: '#e9e9e9',
      },
      tListItem: {
        listStyle: 'none',
        margin: 'auto',
        marginLeft: '1.25em',
        minHeight: '3.125em',
        borderLeft: '0.0625em solid #123bb6',
        padding: '0 0 1.875em 1.875em',
        position: 'relative',
        // Pseudo-class :last-child
        '&:last-child': {
          borderLeft: '0',
        },
        // Pseudo-element ::before
        '&::before': {
          position: 'absolute',
          left: '-18px',
          top: '-5px',
          content: '" "',
          border: '8px solid #d1dbfa',
          borderRadius: '50%',
          backgroundColor: '#123bb6',
          height: '20px',
          width: '20px',
        },
      },
      tStep: {
        color: '#123bb6',
        position: 'relative',
        fontSize: '0.875em',
        marginBottom: '0.5em',
      },
      tCodeBlock: {
        maxHeight: '18.75em',
      },
      tProp: {
        backgroundColor: tokens.colorBrandBackground3Static,
        color: '#333232',
        fontSize: '0.75em',
        padding: '0.1875em 0.625em',
        borderRadius: '0.625em',
        marginBottom: '0.5em',
      },
      citationImg: {
        height: '28.125rem',
        maxWidth: '100%',
        objectFit: 'contain',
      }
});

export const ThoughtProcess = ({ thoughts }: Props) => {
    const classes = useClasses();
    // const jsonstring = JSON.stringify(thoughts, null, 2);

    return (
        <ul className={classes.tList}>
            {thoughts.map((t, ind) => {
                return (
                    <li className={classes.tListItem} key={ind}>
                        <div className={classes.tStep}>{t.title}</div>
                        <Stack horizontal tokens={{ childrenGap: 5 }}>
                            {t.props &&
                                (Object.keys(t.props) || []).map((k: any) => (
                                    <span className={classes.tProp}>
                                        {k}: {JSON.stringify(t.props?.[k])}
                                    </span>
                                ))}
                        </Stack>
                        {Array.isArray(t.description) ? (
                            <ReactJsonView src={t.description}/>
                        ) : (
                            <div>{t.description}</div>
                        )}
                    </li>
                );
            })}
        </ul>
    );
};
